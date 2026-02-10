# BITS Digital Signage Platform — Implementation Plan

Derived from: `digital-signage-platform-specification-v1.11.md`

This plan is organized into phases that respect dependency order.
Each phase contains numbered tasks. Tasks within a phase can often be
parallelized unless a dependency is noted. Each task lists what to
build, its inputs/outputs, and a "done when" checklist.

**Technology stack** (from Spec Section 32):

- Backend: .NET (latest LTS), ASP.NET Core, C#, Entity Framework Core
- Database: PostgreSQL
- Cache: Redis
- Mobile: React Native / Expo, TypeScript
- Player: Kotlin, Android SDK, ExoPlayer
- Storage: Cloud object storage + CDN
- Containers: Docker, orchestration TBD

------------------------------------------------------------------------

# Phase 0 — Project Scaffolding & Infrastructure

Set up repositories, project structure, CI, and shared infrastructure
before writing any domain code.

## 0.1 Repository & Solution Structure

Create the .NET solution with a modular monolith layout.

```
BITS-Signage/
  src/
    Api/                  # ASP.NET Core Web API (user + player endpoints)
    Api.Contracts/        # Request/response DTOs, shared enums
    Domain/               # Domain entities, value objects, interfaces
    Infrastructure/       # EF Core DbContext, repositories, storage, Redis
    Workers/              # Background job host (asset processing, cleanup, propagation)
  tests/
    Api.Tests/
    Domain.Tests/
    Infrastructure.Tests/
    Integration.Tests/
  mobile/                 # Expo/React Native app (Phase 7)
  player/                 # Fire TV Kotlin app (Phase 6)
  db/
    migrations/           # EF Core or raw SQL migrations
  docker/
    docker-compose.yml    # Local dev: PostgreSQL, Redis, storage emulator
```

**Done when:**
- [ ] .NET solution builds with `dotnet build`
- [ ] `docker-compose up` starts PostgreSQL and Redis locally
- [ ] Empty API project starts and returns 200 on `/health`

## 0.2 Database Schema — Core & Auth Tables

Create initial EF Core migrations for the foundational tables that
everything else depends on.

Tables (Spec 26.3, 26.4, 26.5):
- `Tenant` — tenantId, name, status, defaultTimezone, timestamps
- `Venue` — venueId, tenantId (FK), name, timezone, address, fallbackPlaylistId, timestamps
- `User` — userId, tenantId (FK), email, displayName, status, timestamps
- `TenantUserRole` — tenantUserRoleId, tenantId, userId, role, timestamps
- `VenueUserRole` — venueUserRoleId, tenantId, venueId, userId, role, timestamps
- `Device` — deviceId, tenantId, venueId, name, primaryGroupId, timezone, runMode, launchOnBoot, autoRelaunch, keepAwake, watchdogReturnToForeground, orientation, status, lastSeenAtUtc, appVersion, timestamps
- `DeviceGroup` — groupId, tenantId, venueId, name, timestamps
- `DevicePairing` — pairingId, pairingCode, deviceHardwareId, status, expiresAtUtc, claimedByUserId, claimedTenantId, claimedVenueId, resultingDeviceId, timestamps
- `DeviceAuthToken` — deviceAuthTokenId, tenantId, venueId, deviceId, tokenHash, status, issuedAtUtc, timestamps

**Done when:**
- [ ] Migration applies cleanly to empty PostgreSQL database
- [ ] All FK constraints verified
- [ ] Indexes on tenantId, venueId where needed

## 0.3 Database Schema — Content, Playlist, Schedule Tables

Tables (Spec 26.6, 26.7, 26.8, 26.9, 26.10, 26.11, 26.12, 26.13):
- `Asset` — assetId, tenantId, scopeType, scopeVenueId, type, status, displayName, file metadata, checksumSha256, media metadata, cdnUrl, thumbnailUrl, timestamps, deletedAtUtc
- `ContentGroup` — contentGroupId, tenantId, scopeType, scopeVenueId, name, draftVersion, publishedVersion, timestamps, deletedAtUtc
- `ContentGroupItem` — contentGroupItemId, contentGroupId, assetId (nullable), childGroupId (nullable), sequence, durationSeconds, enabled. CHECK: exactly one of assetId/childGroupId non-null
- `ContentGroupPublished` — contentGroupPublishedId, contentGroupId, publishedVersion, publishedAtUtc, publishedByUserId
- `ContentGroupPublishedItem` — contentGroupPublishedItemId, contentGroupId, publishedVersion, assetId (nullable), childGroupId (nullable), childGroupPublishedVersion (nullable), sequence, durationSeconds
- `Playlist` — playlistId, tenantId, scopeType, scopeVenueId, name, draftVersion, publishedVersion, timestamps, deletedAtUtc
- `PlaylistItem` — playlistItemId, playlistId, assetId (nullable), contentGroupId (nullable), sequence, durationSeconds, enabled, timestamps. CHECK: exactly one of assetId/contentGroupId non-null
- `PlaylistPublished` — playlistPublishedId, playlistId, publishedVersion, publishedAtUtc, publishedByUserId
- `PlaylistPublishedItem` — playlistPublishedItemId, playlistId, publishedVersion, assetId, sequence, durationSeconds, sourceContentGroupId (nullable), sourceContentGroupVersion (nullable)
- `VenueScheduleSet` — venueScheduleSetId, tenantId, venueId, draftVersion, publishedVersion, timestamps
- `ScheduleSlot` — slotId, tenantId, venueId, targetType, targetId, playlistId, daysOfWeekMask, startMinutes, endMinutes, priority, validFromDate, validToDate, enabled, timestamps
- `SchedulePublished` — schedulePublishedId, venueId, publishedVersion, publishedAtUtc, publishedByUserId
- `SchedulePublishedSlot` — same fields as ScheduleSlot + publishedVersion
- `VenueEpoch` — venueId (PK), tenantId, manifestEpoch, updatedAtUtc
- `DeviceStatus` — deviceId (PK), venueId, lastHeartbeatAtUtc, isOnline, playback/cache stats, updatedAtUtc
- `DeviceEvent` — deviceEventId, deviceId, eventType, eventTimeUtc, payloadJson, severity, createdAtUtc
- `DeviceCommand` — commandId, deviceId, type, parametersJson, status, issuedByUserId, timestamps, resultJson, error fields
- `PublishEvent` — publishEventId, tenantId, venueId, type (PLAYLIST/SCHEDULE/CONTENT_GROUP), entityId, version fields, publishedByUserId, publishedAtUtc, propagatedPlaylistIds (nullable JSON)
- `EditorPresence` — presenceId, tenantId, resourceType (PLAYLIST/SCHEDULE/CONTENT_GROUP), resourceId, userId, expiresAtUtc

**Done when:**
- [ ] Full schema migration applies cleanly
- [ ] CHECK constraints on PlaylistItem and ContentGroupItem verified
- [ ] Indexes on hot query paths (manifest resolution, device lookup)

## 0.4 API Foundation — Middleware & Cross-Cutting Concerns

Build shared middleware before any endpoint (Spec 25, 27.1, 27.2):

1. **Tenant isolation middleware** — extract tenantId from JWT, inject into request context, enforce on all queries
2. **Authentication middleware** — validate user JWT and device tokens, populate claims
3. **Authorization service** — evaluate tenant roles, venue roles, capability flags per request
4. **Error handling middleware** — problem+json format (Spec 27.2), correlation IDs (`X-Request-Id`)
5. **ETag / If-Match middleware** — 428 if missing, 412 on mismatch for draft-modifying operations
6. **Rate limiting middleware** — per-tenant/user/device limits (Spec 27.1), Redis-backed counters, `X-RateLimit-*` headers, `Retry-After` on 429
7. **Pagination helper** — cursor-based pagination with `?cursor=&limit=` (default 50, max 200)
8. **Filtering helper** — `?q=`, `?status=`, `?scope=`, `?type=` conventions

**Done when:**
- [ ] Unauthenticated request returns 401
- [ ] Cross-tenant request returns 404
- [ ] Missing If-Match returns 428
- [ ] ETag mismatch returns 412
- [ ] Rate limit exceeded returns 429 with Retry-After
- [ ] All error responses match problem+json schema

## 0.5 Storage Service Integration

Configure object storage for asset uploads (Spec 13, 29.4.3):

1. **Storage client** — interface for generating pre-signed upload URLs, download URLs, and deleting objects
2. **CDN URL builder** — generate CDN-prefixed URLs for published assets
3. **Local dev emulator** — MinIO or Azurite in docker-compose for local development

**Done when:**
- [ ] Can generate pre-signed upload URL
- [ ] Can generate CDN download URL for an asset key
- [ ] Works against local emulator in docker-compose

------------------------------------------------------------------------

# Phase 1 — Authentication & Tenant/Venue Management

First real API endpoints. Everything downstream depends on auth and
tenant/venue existing.

## 1.1 User Authentication Endpoints

Spec 25.1–25.3, 27.3:

- `POST /v1/auth/login` — email + password, returns access token (JWT), refresh token, user profile
- `POST /v1/auth/refresh` — exchange refresh token for new access token
- `GET /v1/me` — current user profile and roles
- `GET /v1/me/venues` — list venues user can access

JWT claims: sub, tid, t_roles, v_roles, cap, exp, iss, aud.

**Done when:**
- [ ] Login returns valid JWT with correct claims
- [ ] Refresh works and old access token is rejected after expiry
- [ ] `/me` returns profile with roles
- [ ] `/me/venues` returns only venues user has role for

## 1.2 Tenant Admin Endpoints

Spec 27.4:

- `POST /v1/tenants/{tenantId}/venues` — create venue
- `GET /v1/tenants/{tenantId}/venues` — list venues (paginated)
- `GET /v1/tenants/{tenantId}/venues/{venueId}` — venue detail
- `PUT /v1/tenants/{tenantId}/venues/{venueId}` — update venue (name, timezone, fallback playlist)
- `POST /v1/tenants/{tenantId}/users` — create/invite user
- `GET /v1/tenants/{tenantId}/users` — list users (paginated)
- `GET /v1/tenants/{tenantId}/users/{userId}` — user detail
- `PUT /v1/tenants/{tenantId}/users/{userId}/venue-roles` — assign venue roles

Requires TENANT_ADMIN role.

**Done when:**
- [ ] All 8 endpoints functional
- [ ] Non-admin user gets 403
- [ ] Cross-tenant access returns 404
- [ ] Pagination works on list endpoints

## 1.3 Seed Data & Dev Tooling

Create a seed script for local development:
- 1 tenant with default timezone
- 2 venues
- 1 tenant admin user
- 1 venue manager, 1 venue editor, 1 venue viewer
- Device groups per venue

**Done when:**
- [ ] `dotnet run --seed` (or similar) populates dev database
- [ ] Can log in as any seeded user and see correct venues/roles

------------------------------------------------------------------------

# Phase 2 — Device Management & Pairing

Depends on: Phase 1 (tenants, venues, users exist).

## 2.1 Device Pairing Flow

Spec 8, 26.5, 27.7:

**Backend endpoints:**
- `POST /v1/devices/pairing/start` — player calls with deviceHardwareId, returns pairingCode and pairingId
- `GET /v1/devices/pairing/status` — player polls until status = COMPLETED, receives device token
- `POST /v1/venues/{venueId}/devices/pairing/claim` — mobile user claims pairing code for a venue

**State transitions:** PENDING → CLAIMED → COMPLETED, or PENDING → EXPIRED.

**Done when:**
- [ ] Pairing start returns code and ID
- [ ] Claim assigns device to venue and user
- [ ] Status polling returns COMPLETED with device token
- [ ] Expired pairings return appropriate error
- [ ] Device record created on completion

## 2.2 Device & Group Management Endpoints

Spec 26.4, 27.6:

- `GET /v1/venues/{venueId}/devices` — list devices in venue
- `GET /v1/venues/{venueId}/devices/{deviceId}` — device detail
- `PATCH /v1/venues/{venueId}/devices/{deviceId}` — update name, group, timezone, mode settings
- Device group CRUD under `/v1/venues/{venueId}/groups`

**Done when:**
- [ ] Device list returns devices for venue only
- [ ] PATCH updates device configuration
- [ ] Device group CRUD works
- [ ] Device can be assigned to a group

## 2.3 Device Token Authentication

Spec 25.2 (device auth), 25.3 (device token claims):

- Issue device tokens on pairing completion
- Validate device tokens on player endpoints
- Device token contains: typ, did, vid, tid, exp
- Invalidate on device removal or venue reassignment

**Done when:**
- [ ] Device token validates on player endpoints
- [ ] Invalid/expired token returns 401
- [ ] Token invalidated when device is removed

------------------------------------------------------------------------

# Phase 3 — Content Management (Assets)

Depends on: Phase 1 (tenants, venues), Phase 0.5 (storage).

## 3.1 Asset Upload Pipeline

Spec 13, 13.1–13.5, 27.5, 27.6:

**Endpoints:**
- `POST /v1/library/assets/uploads` — create upload session (tenant-scoped), returns pre-signed URL
- `POST /v1/venues/{venueId}/assets/uploads` — create upload session (venue-scoped)
- `POST /v1/library/assets/uploads/{uploadId}/complete` — signal upload finished
- `POST /v1/venues/{venueId}/assets/uploads/{uploadId}/complete` — signal venue upload finished

**Flow:**
1. Client requests upload session → gets pre-signed URL
2. Client uploads file directly to storage
3. Client calls `/complete` → asset status becomes PROCESSING
4. Background worker validates, extracts metadata, generates thumbnail
5. Asset becomes READY or FAILED (with failureReason and failureDetail)

**Done when:**
- [ ] Upload session returns pre-signed URL
- [ ] Complete callback triggers processing
- [ ] Asset transitions through UPLOADING → PROCESSING → READY
- [ ] Invalid media results in FAILED with appropriate reason code

## 3.2 Asset Processing Worker

Spec 13.1–13.5, 29.5:

Background worker that processes uploaded assets:

1. Validate format (JPEG, PNG, MP4)
2. Validate codec (H.264 video, AAC audio)
3. Validate constraints (≤1920x1080, ≤30fps, ≤10Mbps, ≤120s, file size)
4. Extract metadata (dimensions, duration, file size)
5. Compute SHA-256 checksum
6. Generate thumbnail
7. Set cdnUrl and thumbnailUrl
8. Mark READY or FAILED

**Failure reasons** (Spec 13.2): INVALID_FORMAT, INVALID_CODEC, RESOLUTION_EXCEEDED, DURATION_EXCEEDED, BITRATE_EXCEEDED, FILE_SIZE_EXCEEDED, FILE_CORRUPT, CHECKSUM_MISMATCH, THUMBNAIL_FAILED, PROCESSING_TIMEOUT, INTERNAL_ERROR.

**Timeouts** (Spec 13.5): UPLOADING 60min, PROCESSING 10min.

**Done when:**
- [ ] Valid JPEG/PNG → READY with metadata
- [ ] Valid MP4 (H.264/AAC) → READY with metadata and duration
- [ ] VP9 video → FAILED with INVALID_CODEC
- [ ] Oversized file → FAILED with appropriate reason
- [ ] Timeout enforced for stuck processing

## 3.3 Asset CRUD Endpoints

Spec 27.5, 27.6:

- `GET /v1/library/assets` — list tenant assets (paginated, filterable)
- `GET /v1/library/assets/{assetId}` — asset detail (includes status, metadata, thumbnail)
- `DELETE /v1/library/assets/{assetId}` — soft delete
- `GET /v1/venues/{venueId}/assets` — list venue assets
- `GET /v1/venues/{venueId}/assets/{assetId}` — venue asset detail
- `DELETE /v1/venues/{venueId}/assets/{assetId}` — soft delete venue asset

Filters: `?status=READY`, `?type=IMAGE|VIDEO`, `?scope=TENANT|VENUE`, `?q=search`

**Done when:**
- [ ] List returns paginated, filterable results
- [ ] Venue assets include both venue-specific and tenant-shared
- [ ] Soft delete sets deletedAtUtc, asset no longer appears in lists
- [ ] Status polling works (for upload progress tracking)

## 3.4 Cleanup Worker — Assets

Spec 13.5, 29.5:

- FAILED assets: retained 30 days then hard-deleted with storage files
- UPLOADING assets that timeout (60 min): marked FAILED
- Unused upload sessions (no file received): expired after 60 min
- Storage cleanup runs hourly

**Done when:**
- [ ] Stale UPLOADING assets transition to FAILED after timeout
- [ ] FAILED assets older than 30 days are hard-deleted
- [ ] Associated storage files are removed on hard delete

------------------------------------------------------------------------

# Phase 4 — Content Groups

Depends on: Phase 3 (assets exist to reference).

## 4.1 Content Group CRUD

Spec 6 (Content Group), 26.7, 27.5, 27.6:

**Library (tenant-scoped) endpoints:**
- `POST /v1/library/content-groups` — create
- `GET /v1/library/content-groups` — list (paginated)
- `GET /v1/library/content-groups/{contentGroupId}` — detail with draft items
- `PUT /v1/library/content-groups/{contentGroupId}` — update name/metadata
- `DELETE /v1/library/content-groups/{contentGroupId}` — soft delete

**Venue-scoped endpoints** — same pattern under `/v1/venues/{venueId}/content-groups/...`

**Done when:**
- [ ] CRUD works for both tenant and venue scoped groups
- [ ] Soft delete hides from lists
- [ ] ETag returned on GET, If-Match required on mutations

## 4.2 Content Group Item Management

Spec 26.7 (ContentGroupItem), 27.5, 27.6:

- `PUT /v1/library/content-groups/{contentGroupId}/items` — replace draft items
- `PUT /v1/library/content-groups/{contentGroupId}/reorder` — reorder items
- Same for venue-scoped

**Item structure:** Each item has either `assetId` OR `childGroupId` (never both).
- `assetId` — references an asset
- `childGroupId` — references another content group (nesting)
- `sequence` — display order
- `durationSeconds` — optional override
- `enabled` — boolean

**Validation (Spec 27.10):**
- Mutual exclusivity: exactly one of assetId/childGroupId per item
- Max nesting depth: 3 levels
- Cycle detection: depth-limited traversal of childGroupId references. Return 422 `CYCLE_DETECTED` on violation.
- Referenced assets must exist (not deleted)
- Referenced child groups must exist and belong to same tenant

**Done when:**
- [ ] Items saved with correct sequence
- [ ] assetId XOR childGroupId enforced (422 on violation)
- [ ] Nesting beyond depth 3 rejected (422)
- [ ] Circular reference detected and rejected (422 CYCLE_DETECTED)
- [ ] Draft version incremented on change

## 4.3 Content Group Publishing

Spec 15.2, 26.7, 27.11:

- `POST /v1/library/content-groups/{contentGroupId}/publish`
- `POST /v1/venues/{venueId}/content-groups/{contentGroupId}/publish`

**Request body:** `{ "propagate": true|false }`

**Publish flow:**
1. Validate all referenced assets are READY
2. Validate all referenced child groups have a published version
3. Snapshot draft items → ContentGroupPublished + ContentGroupPublishedItem
4. Increment publishedVersion
5. Record PublishEvent (type: CONTENT_GROUP)

**If `propagate: true`:**
6. Find all playlists that reference this content group (via PlaylistItem.contentGroupId)
7. For each playlist: re-publish with expanded group items
8. Increment venue epochs for affected venues
9. Return propagationSummary: `{ propagated, playlistsUpdated, playlistsFailed, playlistIds }`

**If `propagate: false`:**
6. No downstream action. Playlists updated on next manual publish.

**Rate limiting note** (Spec 27.1): auto-propagate counts as single publish operation.

**Done when:**
- [ ] Publish creates immutable snapshot
- [ ] `propagate: true` re-publishes all referencing playlists
- [ ] `propagate: false` leaves playlists unchanged
- [ ] propagationSummary returned correctly
- [ ] PublishEvent recorded with propagatedPlaylistIds
- [ ] Venue epochs incremented for affected venues

## 4.4 Content Group Rollback

Spec 27.9:

- `POST /v1/library/content-groups/{contentGroupId}/rollback`
- `POST /v1/venues/{venueId}/content-groups/{contentGroupId}/rollback`

Restore a previous published version from ContentGroupPublished history.

**Done when:**
- [ ] Rollback restores previous published version
- [ ] Version number increments (not reverts)
- [ ] PublishEvent recorded
- [ ] Venue epoch incremented

## 4.5 Content Group Presence

Spec 26.13:

Track which users are editing a content group (EditorPresence with resourceType = CONTENT_GROUP).

**Done when:**
- [ ] Presence recorded when user opens content group for editing
- [ ] Active editors visible to other users
- [ ] Presence expires automatically

------------------------------------------------------------------------

# Phase 5 — Playlists

Depends on: Phase 3 (assets), Phase 4 (content groups).

## 5.1 Playlist CRUD

Spec 26.8, 27.5, 27.6:

**Library endpoints:**
- `POST /v1/library/playlists`
- `GET /v1/library/playlists`
- `GET /v1/library/playlists/{playlistId}`

**Venue endpoints** — same pattern under `/v1/venues/{venueId}/playlists/...`

**Done when:**
- [ ] Playlist created with name, scope (TENANT or VENUE)
- [ ] List returns paginated results
- [ ] Detail includes draft items
- [ ] ETag concurrency on reads/writes

## 5.2 Playlist Item Management

Spec 26.8 (PlaylistItem):

- `PUT /v1/library/playlists/{playlistId}/items`
- `PUT /v1/library/playlists/{playlistId}/reorder`
- Same for venue-scoped

**Item structure:** Each item has either `assetId` OR `contentGroupId`:
- `assetId` — direct asset reference
- `contentGroupId` — content group reference (expanded at publish time)
- `sequence`, `durationSeconds`, `enabled`

**Validation:**
- Mutual exclusivity: exactly one of assetId/contentGroupId
- Referenced assets must exist and not be deleted
- Referenced content groups must exist and not be deleted

**Done when:**
- [ ] Items saved with asset or content group references
- [ ] XOR constraint enforced
- [ ] Reorder updates sequence values
- [ ] Draft version incremented

## 5.3 Playlist Publishing

Spec 15.1, 26.8:

- `POST /v1/library/playlists/{playlistId}/publish`
- `POST /v1/venues/{venueId}/playlists/{playlistId}/publish`

**Publish flow:**
1. Validate all directly-referenced assets are READY
2. Validate all referenced content groups have published versions
3. **Expand content groups:** For each item with contentGroupId, recursively resolve to flat asset list using the group's published items. Record sourceContentGroupId and sourceContentGroupVersion on each expanded row.
4. Snapshot to PlaylistPublished + PlaylistPublishedItem (always flat assets — no group references in published items)
5. Increment publishedVersion
6. Increment venue epoch(s) for affected venues
7. Record PublishEvent

**Done when:**
- [ ] Published items are always flat asset references
- [ ] Content group items expanded correctly (including nested groups)
- [ ] sourceContentGroupId/Version recorded for audit
- [ ] All referenced assets validated as READY
- [ ] Venue epoch incremented
- [ ] PublishEvent recorded

## 5.4 Playlist Rollback

Spec 27.9:

- `POST /v1/library/playlists/{playlistId}/rollback`
- `POST /v1/venues/{venueId}/playlists/{playlistId}/rollback`

**Done when:**
- [ ] Rollback restores previous published snapshot
- [ ] Version increments forward
- [ ] Venue epoch incremented

## 5.5 Playlist Presence

EditorPresence with resourceType = PLAYLIST.

**Done when:**
- [ ] Active editors tracked and visible

------------------------------------------------------------------------

# Phase 6 — Scheduling

Depends on: Phase 5 (playlists must exist and be publishable).

## 6.1 Schedule Slot Management

Spec 7, 26.9, 27.6:

- `GET /v1/venues/{venueId}/scheduleset` — current schedule with draft slots
- `POST /v1/venues/{venueId}/scheduleset/slots` — create slot
- `PUT /v1/venues/{venueId}/scheduleset/slots/{slotId}` — update slot
- `DELETE /v1/venues/{venueId}/scheduleset/slots/{slotId}` — delete slot

**Slot fields:** targetType (DEVICE/GROUP), targetId, playlistId, daysOfWeekMask (bitmask), startMinutes (0–1439), endMinutes (0–1439), priority, validFromDate, validToDate, enabled.

**Midnight wrap:** Supported when endMinutes ≤ startMinutes.

**Done when:**
- [ ] Slots created with all fields
- [ ] Midnight-wrap slots accepted
- [ ] ETag concurrency on schedule set
- [ ] Draft version incremented on changes

## 6.2 Schedule Publishing

Spec 15.3, 26.9:

- `POST /v1/venues/{venueId}/scheduleset/publish`

**Validation:** All referenced playlists must have published versions.

**Publish flow:**
1. Snapshot draft slots → SchedulePublished + SchedulePublishedSlot
2. Increment publishedVersion
3. Increment venue epoch
4. Record PublishEvent

**Done when:**
- [ ] Publish creates immutable snapshot
- [ ] Unpublished playlist reference rejected (422)
- [ ] Venue epoch incremented

## 6.3 Schedule Rollback

- `POST /v1/venues/{venueId}/scheduleset/rollback`

**Done when:**
- [ ] Previous schedule version restored
- [ ] Venue epoch incremented

## 6.4 Schedule Resolver Algorithm

Spec 31 (Schedule Resolver), 30 (Timezone Handling):

Core algorithm that determines which playlist a device should play
right now:

**Inputs:** deviceId, current UTC time

**Algorithm:**
1. Look up device → venue → tenant to get effective timezone (device > venue > tenant)
2. Convert UTC now to local time in effective timezone
3. Extract local day-of-week and minutes-since-midnight
4. Load published schedule slots for this venue
5. Filter to matching slots:
   - enabled = true
   - daysOfWeekMask includes current local day
   - Current local time within [startMinutes, endMinutes] (handle midnight wrap)
   - Date range includes today (if validFrom/validTo set)
   - Target matches device directly OR device's primary group
6. Apply precedence:
   - Device-targeted slots beat group-targeted slots
   - Higher priority wins
   - Latest updatedAtUtc breaks ties
   - Stable ID ordering as final tiebreaker
7. Return winning playlist ID and published version
8. If no match: use venue fallbackPlaylistId, else null
9. Compute validToUtc (nearest future boundary, clamped to max ~6 hours)

**Performance target:** <5ms per resolution.

**Done when:**
- [ ] Correct playlist resolved for device-targeted slot
- [ ] Group-targeted slot works when device has no direct slot
- [ ] Device slot overrides group slot
- [ ] Priority ordering works
- [ ] Midnight-wrap slots resolve correctly
- [ ] DST transitions handled (no oscillation)
- [ ] Fallback playlist used when no slot matches
- [ ] validToUtc computed correctly
- [ ] Unit tests cover all edge cases

------------------------------------------------------------------------

# Phase 7 — Player Endpoints & Manifest

Depends on: Phase 6 (schedule resolver), Phase 5 (published playlists), Phase 2 (device auth).

## 7.1 Manifest Endpoint

Spec 27.8.1, 12, 29.4.8:

`GET /v1/player/manifest` — device token auth required

**Response building:**
1. Authenticate device token → extract deviceId, venueId, tenantId
2. Check venue epoch for cache hit (Redis)
3. Run schedule resolver → winning playlist
4. Build manifest JSON:
   - `manifestEtag` — e.g., `"e:{venueId}:{epoch}"`
   - `validToUtc` — from resolver
   - `nextCheckSeconds` — e.g., 60
   - `settings` — from Device record (runMode, launchOnBoot, autoRelaunch, keepAwake, watchdog, orientation)
   - `playlist` — null if no match, otherwise: playlistId, publishedVersion, items[]
   - Each item: assetId, type, url (CDN), checksumSha256, durationSeconds, widthPx, heightPx, fileSizeBytes
5. Support `If-None-Match` → 304 Not Modified when ETag matches
6. Cache result briefly in Redis (TTL 30–60s, keyed by venue+device+epoch)

**Done when:**
- [ ] Manifest returns correct playlist for device
- [ ] Settings reflect device configuration
- [ ] 304 returned when ETag matches
- [ ] Null playlist when no schedule match and no fallback
- [ ] CDN URLs in asset items
- [ ] Response cached in Redis

## 7.2 Heartbeat Endpoint

Spec 27.8.2:

`POST /v1/player/heartbeat` — device token auth

Accept heartbeat payload, update DeviceStatus record.
Response: 204 No Content.

**Done when:**
- [ ] DeviceStatus updated with playback state, cache stats, network info
- [ ] lastHeartbeatAtUtc set
- [ ] isOnline derived from heartbeat recency

## 7.3 Event Batch Endpoint

Spec 27.8.3:

`POST /v1/player/events` — device token auth

Accept batch of up to 100 events, append to DeviceEvent table.
Response: 204 No Content.

**Done when:**
- [ ] Events stored with correct type, time, severity, payload
- [ ] Batch >100 rejected
- [ ] Rate limited per device (6/min)

## 7.4 Command Endpoints

Spec 27.6, 27.8:

- `POST /v1/venues/{venueId}/devices/{deviceId}/commands` — admin issues command
- `GET /v1/player/commands` — device polls for pending commands
- `POST /v1/player/commands/{commandId}/result` — device reports result

Command types: REQUEST_MANIFEST_REFRESH, RESTART_PLAYBACK_LOOP, CLEAR_CACHE_UNUSED, CLEAR_CACHE_ALL, REBOOT_APP, SET_LOG_LEVEL.

**Done when:**
- [ ] Admin can issue commands
- [ ] Device receives pending commands on poll
- [ ] Device can report results
- [ ] Command status tracked (PENDING → EXECUTED/FAILED)

## 7.5 Publishing History Endpoint

Spec 27.6:

`GET /v1/venues/{venueId}/publishes` — paginated publish event history

**Done when:**
- [ ] Returns publish events for venue (playlists, schedules, content groups)
- [ ] Paginated with cursor

------------------------------------------------------------------------

# Phase 8 — Fire TV Player App

Depends on: Phase 7 (all player endpoints available).

## 8.1 Player App Scaffolding

Kotlin Android project targeting Fire TV:
- Project structure, build configuration
- Networking layer (OkHttp/Retrofit) with device token auth
- Local storage (SQLite via Room)
- Dependency injection setup

**Done when:**
- [ ] App builds and installs on Fire TV device/emulator
- [ ] Network client configured with base URL and auth header

## 8.2 Pairing Screen & Flow

Spec 8, 28.4 (BOOT → UNREGISTERED):

1. Generate/display pairing QR code (containing pairingCode + deviceHardwareId)
2. Call `POST /v1/devices/pairing/start`
3. Poll `GET /v1/devices/pairing/status` until COMPLETED
4. Store received device token securely
5. Transition to playback mode

**Done when:**
- [ ] QR code displayed with pairing info
- [ ] Polling detects claim and retrieves token
- [ ] Token stored in secure app storage
- [ ] Transitions to manifest fetch on completion

## 8.3 State Machine Implementation

Spec 28.3–28.4:

Implement primary states: BOOT → LOAD_LOCAL → PLAYABLE_CHECK → PLAYING / ERROR_SCREEN.

Plus background states: SYNC_MANIFEST, SYNC_COMMANDS, DOWNLOAD_ASSETS, APPLY_STAGED.

**Done when:**
- [ ] Boot initializes all modules
- [ ] Local manifest loaded from storage
- [ ] Playable check evaluates cached assets
- [ ] Transitions match spec state table (28.12)

## 8.4 Manifest Sync Loop

Spec 28.6:

Background coroutine that:
1. Polls `GET /v1/player/manifest` at `nextCheckSeconds` interval
2. Handles 304 (no change), 200 (new manifest), 401 (re-pair), 429/503 (backoff)
3. On new manifest: validate, stage, queue asset downloads
4. Exponential backoff on failures
5. Emits telemetry events (MANIFEST_FETCH_FAILED, ONLINE_RESTORED, OFFLINE_ENTERED)

**Done when:**
- [ ] Periodic polling works at configured interval
- [ ] 304 response handled (no action)
- [ ] New manifest staged correctly
- [ ] Network failure triggers backoff
- [ ] Telemetry events emitted

## 8.5 Asset Download & Cache Manager

Spec 9, 11, 28.9:

1. **Download manager:** Priority queue (staged > upcoming > remaining), checksum verification, temp file → atomic move
2. **Cache manager:** 2GB default budget, LRU eviction at 90% usage, protect current+previous manifest assets
3. **Cache index:** SQLite tracking asset paths, checksums, timestamps

**Done when:**
- [ ] Assets downloaded from CDN URLs in manifest
- [ ] SHA-256 verified after download
- [ ] Cache eviction frees space when needed
- [ ] Current/previous manifest assets protected from eviction
- [ ] Downloads pause when cache full and eviction insufficient

## 8.6 Playback Engine

Spec 9, 28.4 (PLAYING):

1. ExoPlayer integration for video playback
2. ImageView for image display with duration timer
3. Ordered rotation through playlist items
4. Skip missing/failed assets
5. Videos play to completion (no mid-video interrupt)
6. Manifest swap only at safe boundaries (between items or end of loop)
7. Loop continuously
8. Never blank screen if cached content available

**Done when:**
- [ ] Images display for configured duration
- [ ] Videos play through fully
- [ ] Missing assets skipped without blank
- [ ] Playlist loops continuously
- [ ] Manifest update applied at safe boundary

## 8.7 Staged Manifest Apply

Spec 28.5:

Two-phase update: CURRENT manifest stays active while STAGED downloads assets.
Swap CURRENT ← STAGED at safe boundaries.

States: STAGED_NONE, STAGED_WAITING_ASSETS, STAGED_READY, STAGED_REJECTED.

Persist current and previous manifests to local storage for offline recovery.

**Done when:**
- [ ] Staged manifest downloads assets in background
- [ ] Swap happens only at safe boundaries
- [ ] Previous manifest preserved for fallback
- [ ] Invalid staged manifest rejected

## 8.8 Telemetry — Heartbeat & Events

Spec 17, 27.8.2, 27.8.3, 28.8:

1. **Heartbeat:** Send every ~60s with playback state, cache stats, network status, error counts
2. **Event buffer:** Queue events locally (SQLite), batch flush (max 100), retry with backoff
3. Event types per spec (APP_STARTED, PLAYBACK_FAILED, DOWNLOAD_FAILED, etc.)

**Done when:**
- [ ] Heartbeat sent at configured interval
- [ ] Events buffered and batched
- [ ] Failed sends retry with backoff
- [ ] Events >7 days old discarded locally

## 8.9 Command Execution

Spec 18, 28.7:

Poll `GET /v1/player/commands` periodically. Execute at safe boundaries:
- REQUEST_MANIFEST_REFRESH — trigger immediate manifest poll
- RESTART_PLAYBACK_LOOP — restart from first item
- CLEAR_CACHE_UNUSED — evict non-protected assets
- CLEAR_CACHE_ALL — clear entire cache, re-download
- REBOOT_APP — restart application
- SET_LOG_LEVEL — adjust logging verbosity

Report results via `POST /v1/player/commands/{commandId}/result`.

**Done when:**
- [ ] Commands polled and executed
- [ ] Results reported back
- [ ] Last processed command persisted (no re-execution)

## 8.10 Kiosk & Shared Mode

Spec 19, 28.11:

- **Shared mode:** Launch on boot (optional), user may exit
- **Kiosk mode:** Auto-relaunch, force foreground, keep screen awake, watchdog

Settings driven from manifest `settings` object.

**Done when:**
- [ ] Shared mode allows normal exit
- [ ] Kiosk mode prevents exit, keeps screen on
- [ ] Mode switches applied when manifest updates

## 8.11 Error & Fallback Screens

Spec 28.4 (ERROR_SCREEN):

- Display when no playable content exists
- Show helpful message
- Continue sync attempts in background
- Transition to PLAYING when content becomes available

**Done when:**
- [ ] Error screen shown when no cached content
- [ ] Background sync continues
- [ ] Auto-transition to playback when content available

------------------------------------------------------------------------

# Phase 9 — Mobile Manager App

Depends on: Phase 7 (all backend endpoints), can be developed in
parallel with Phase 8 (player).

## 9.1 Mobile App Scaffolding

Expo/React Native project with TypeScript:
- Navigation structure (tab-based or stack)
- API client with JWT auth, token refresh, error handling
- Secure token storage
- Shared components library

**Done when:**
- [ ] App runs on iOS simulator and Android emulator
- [ ] API client configured with base URL
- [ ] Auth flow works (login, token storage, refresh)

## 9.2 Authentication & Venue Selection

Screens:
- Login screen (email + password)
- Venue picker (from `/v1/me/venues`)
- Venue switching

**Done when:**
- [ ] Login works with error handling
- [ ] Venue list shows accessible venues
- [ ] Selected venue persisted across sessions

## 9.3 Device Management Screens

Screens:
- Device list (per venue, with online/offline indicators)
- Device detail (status, health, config)
- Device configuration editor (name, group, mode, orientation)
- Remote command panel

**Done when:**
- [ ] Device list shows real-time status
- [ ] Config changes saved via PATCH
- [ ] Commands can be issued and results viewed

## 9.4 Device Pairing Screen

- QR code scanner (camera permission)
- Claim flow (`POST /v1/venues/{venueId}/devices/pairing/claim`)
- Success confirmation

**Done when:**
- [ ] QR scanner reads pairing code
- [ ] Device claimed and appears in venue device list

## 9.5 Asset Library Screens

Screens:
- Asset list (filterable by type, status, scope)
- Asset upload (file picker, progress indicator, status polling)
- Asset detail (metadata, thumbnail, status)

**Done when:**
- [ ] Assets listed with thumbnails
- [ ] Upload shows progress and final status
- [ ] Failed uploads show failure reason with retry option

## 9.6 Content Group Screens

Screens:
- Content group list (tenant + venue)
- Content group editor (add/remove/reorder assets and nested groups)
- Publish dialog (propagate yes/no)
- Publish history

**Done when:**
- [ ] Groups created, edited, reordered
- [ ] Nested groups visible with depth indicator
- [ ] Publish with propagation option works
- [ ] Propagation summary shown after publish

## 9.7 Playlist Screens

Screens:
- Playlist list (tenant + venue)
- Playlist editor (add/remove/reorder items — assets or content groups)
- Publish button
- Publish history

**Done when:**
- [ ] Playlists created and edited
- [ ] Items can be assets or content groups
- [ ] Publish creates snapshot
- [ ] Presence indicators show other editors

## 9.8 Schedule Screens

Screens:
- Schedule overview (slot list or timeline view)
- Slot editor (target, playlist, days, time window, priority, date range)
- Publish button
- Publish history

**Done when:**
- [ ] Schedule slots created and edited
- [ ] Time picker works with midnight-wrap support
- [ ] Publish works with validation feedback

## 9.9 Publish History & Rollback

Screens:
- Publish history timeline (all types: playlist, schedule, content group)
- Rollback action with confirmation

**Done when:**
- [ ] History shows all publish events
- [ ] Rollback works for all resource types
- [ ] Confirmation dialog prevents accidental rollback

------------------------------------------------------------------------

# Phase 10 — Operations Console

Depends on: Phase 7 (backend endpoints).

## 10.1 Operations Dashboard

Web UI providing:
- Device status overview (online/offline counts per venue)
- Playback health summary
- Recent alerts and errors
- System health indicators

**Done when:**
- [ ] Dashboard shows real-time device status
- [ ] Error trends visible

## 10.2 Device Management Console

Web UI for:
- Device detail with full telemetry
- Command history and issuance
- Configuration management
- Event log viewer

**Done when:**
- [ ] Full device detail visible
- [ ] Commands issuable from console

## 10.3 Content & Publishing Management

Web UI for:
- Asset library browser
- Content group management
- Playlist management
- Schedule management
- Publishing history with rollback

**Done when:**
- [ ] All content CRUD available
- [ ] Publishing and rollback functional

## 10.4 Alerting & Monitoring Setup

- Offline device detection (no heartbeat for configurable threshold)
- Error pattern flagging (repeated download/playback failures)
- Alert rules and notification channels

**Done when:**
- [ ] Offline devices flagged
- [ ] Error patterns detected
- [ ] Alerts surfaced in dashboard

------------------------------------------------------------------------

# Phase 11 — Background Workers & Operational Hardening

Depends on: All prior phases functional.

## 11.1 Cleanup Workers

Spec 13.5, 29.5:

Scheduled jobs:
- Expire pairing codes (PENDING → EXPIRED)
- Expire upload sessions (60 min)
- Timeout UPLOADING assets (60 min) → mark FAILED
- Hard-delete FAILED assets (30 day retention)
- Clean orphaned storage files
- Expire presence records
- Telemetry retention/aggregation (configurable)

**Done when:**
- [ ] All cleanup jobs run on schedule
- [ ] Stale records cleaned up correctly
- [ ] Storage files removed for deleted assets

## 11.2 Content Group Propagation Worker

Spec 29.5:

Handles async propagation when content group published with `propagate: true`:
1. Receive propagation event
2. Find all playlists referencing the content group
3. Re-publish each with expanded items
4. Track success/failure per playlist
5. Increment venue epochs

Note: This may already be handled synchronously in Phase 4.3. This
task covers making it robust for large numbers of referencing playlists
(async/queued processing).

**Done when:**
- [ ] Large propagation sets handled without timeout
- [ ] Failures tracked per playlist
- [ ] Partial failures don't block other playlists

## 11.3 Observability Setup

- Structured logging (JSON) with correlation IDs
- Metrics: API latency, error rates, device counts, publish throughput
- Health check endpoints (`/health`, `/ready`)
- Integration with observability stack (CloudWatch, Datadog, etc.)

**Done when:**
- [ ] Logs include correlation IDs and tenant context
- [ ] Key metrics exported
- [ ] Health checks pass for all dependencies

------------------------------------------------------------------------

# Phase 12 — Deployment & Scaling

## 12.1 Containerization

- Dockerfile for API service
- Dockerfile for Worker service
- docker-compose for full local stack (API, Worker, PostgreSQL, Redis, MinIO)

**Done when:**
- [ ] Full stack runs via docker-compose
- [ ] Images build cleanly in CI

## 12.2 Production Deployment Configuration

- Kubernetes manifests or cloud-native service configs
- Separate scaling for User API and Player Edge API
- Worker autoscaling based on queue depth
- Database connection pooling
- Redis replication/failover
- CDN configuration

**Done when:**
- [ ] Deployment runs in target cloud environment
- [ ] Player endpoints scale independently
- [ ] Zero-downtime deployments work

## 12.3 CI/CD Pipeline

- Build, test, lint on PR
- Integration test suite
- Container image build and push
- Automated deployment to staging
- Manual promotion to production

**Done when:**
- [ ] PR checks run automatically
- [ ] Merge to main deploys to staging
- [ ] Production deployment requires approval

------------------------------------------------------------------------

# Appendix A — Task Dependency Graph

```
Phase 0  (scaffolding, schema, middleware, storage)
  │
  ├─► Phase 1  (auth, tenant/venue management)
  │     │
  │     ├─► Phase 2  (device management, pairing)
  │     │     │
  │     │     └─► Phase 7  (player endpoints — needs device auth)
  │     │           │
  │     │           ├─► Phase 8  (Fire TV player app)
  │     │           └─► Phase 10 (operations console)
  │     │
  │     └─► Phase 3  (asset management)
  │           │
  │           └─► Phase 4  (content groups)
  │                 │
  │                 └─► Phase 5  (playlists)
  │                       │
  │                       └─► Phase 6  (scheduling)
  │                             │
  │                             └─► Phase 7  (manifest resolver)
  │
  ├─► Phase 9  (mobile app — can start UI shell early,
  │             endpoints needed for integration)
  │
  ├─► Phase 11 (workers & hardening — after all features)
  │
  └─► Phase 12 (deployment — after all features)
```

**Parallelism opportunities:**
- Phase 8 (player) and Phase 9 (mobile) can be developed in parallel once Phase 7 is ready
- Phase 3 (assets) and Phase 2 (devices) can be developed in parallel after Phase 1
- Phase 10 (ops console) can be developed alongside Phases 8/9
- Mobile app UI shell (Phase 9.1–9.2) can start during Phase 3–6

------------------------------------------------------------------------

# Appendix B — Milestone Mapping

Mapping to Spec Section 23 milestones:

| Milestone | Phases | Description |
|-----------|--------|-------------|
| M0 | 0, 1, 2 | Tenant, venue, device pairing |
| M1 | 3, 5, 7, 8 | Playback MVP with caching |
| M2 | 4, 6, 9, 10 | Content groups, scheduling, operations |
| M3 | 11 | Operational hardening |
| M4 | 12 | SaaS scaling |
