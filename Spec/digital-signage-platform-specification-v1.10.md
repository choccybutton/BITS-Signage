# Digital Signage Platform

## Full Product & Technical Specification (Current State)

Version: 1.10 (Consolidated to current design state)

------------------------------------------------------------------------

# 1. System Overview

This platform provides a **multi-tenant, multi-venue digital signage
solution** allowing venues to display advertising and promotional
content on Fire TV devices, managed through mobile applications and
backend services.

The system supports:

-   Multi-tenant SaaS operation
-   Multiple venues per tenant
-   Per-device content playback
-   Time-of-day scheduling
-   Playlist rotation
-   Offline-safe playback
-   Remote device management
-   Tenant-shared and venue-specific content
-   Device health monitoring
-   Publishing workflows
-   Operational dashboards

The solution consists of:

1.  Fire TV Player App
2.  Mobile Manager App (Expo)
3.  Backend API & services
4.  Storage + CDN
5.  Operations console

Out of scope for this specification:

-   Synchronized playback across multiple devices (video walls).
-   Ad impression tracking and reporting.
-   Analytics dashboards beyond operational health.

------------------------------------------------------------------------

# 2. Logical Architecture

    Platform
      └── Tenant
            └── Venue
                  └── Devices

Example:

    Tenant: Hospitality Group
      Venue: Fortescue Arms
      Venue: Grumpy Badger Café

Each venue contains:

-   Devices
-   Device groups
-   Schedules
-   Overrides/events
-   Venue-specific content

Tenant contains:

-   Users
-   Shared content library
-   Playlists
-   Venues

------------------------------------------------------------------------

# 3. System Components

## 3.1 Fire TV Player App

Runs on Fire TV devices.

Responsibilities:

-   Fullscreen playback
-   Manifest polling
-   Asset caching
-   Offline playback
-   Telemetry reporting
-   Command execution
-   Device registration
-   Health reporting

------------------------------------------------------------------------

## 3.2 Mobile Manager App

Used by staff to manage venues.

Features:

-   Device management
-   Asset upload
-   Playlist editing
-   Schedule management
-   Publish workflows
-   Device health view
-   Remote support commands

------------------------------------------------------------------------

## 3.3 Backend Services

Provide:

-   Authentication
-   Authorization
-   Content management
-   Scheduling engine
-   Manifest resolution
-   Telemetry ingestion
-   Device command delivery

------------------------------------------------------------------------

## 3.4 Storage + CDN

Responsible for:

-   Asset storage
-   Media delivery
-   High availability distribution

------------------------------------------------------------------------

# 4. Multi-Tenant & Venue Model

## Tenant

Represents a customer organization.

Contains:

-   Users
-   Venues
-   Shared assets
-   Shared playlists

------------------------------------------------------------------------

## Venue

Represents a physical location.

Contains:

-   Devices
-   Groups
-   Schedules
-   Venue content

------------------------------------------------------------------------

## Devices

Fire TV players registered to venues.

Store:

-   Device configuration
-   Cache
-   Playback state
-   Telemetry data

------------------------------------------------------------------------

# 5. Roles & Permissions

## Tenant Roles

Tenant Admin: - Full tenant control - User management - Venue
management - Shared content control

Tenant Viewer: - Read-only tenant view

------------------------------------------------------------------------

## Venue Roles

Venue Manager: - Manage devices - Manage schedules - Publish content -
Run support commands

Venue Editor: - Manage playlists - Manage schedules - Upload content

Venue Viewer: - Read-only access

Users may belong to multiple venues.

------------------------------------------------------------------------

# 6. Content Model

## Asset

Media content items.

Supported formats:

Images: - JPEG - PNG

Video: - MP4 container - H.264 video - AAC audio

Asset lifecycle:

-   DRAFT
-   UPLOADING
-   PROCESSING
-   READY
-   FAILED

Only READY assets usable in playlists.

------------------------------------------------------------------------

## Playlist

Ordered sequence of assets.

Features:

-   Ordered rotation
-   Per-item duration
-   Draft & Published versions
-   ETag concurrency
-   Presence indicators

------------------------------------------------------------------------

## Content Scope

Assets and playlists may be:

-   TENANT scoped (shared)
-   VENUE scoped (local)

Venues see tenant-shared + venue-local content.

------------------------------------------------------------------------

# 7. Scheduling Model

Schedules assign playlists by:

-   Device or group
-   Day of week
-   Time window
-   Priority
-   Optional date range

Resolution rules:

1.  Device rules override group rules.
2.  Highest priority wins.
3.  Latest updated resolves ties.
4.  Fallback playlist used when none match.

Wrap windows supported across midnight.

------------------------------------------------------------------------

# 8. Device Registration

Flow:

1.  Player shows pairing QR code.
2.  Mobile app scans code.
3.  Device assigned to venue.
4.  Device receives authentication token.

------------------------------------------------------------------------

# 9. Playback Engine

Playback sequence:

1.  Load cached manifest.
2.  Start playback immediately.
3.  Fetch new manifest in background.
4.  Download missing assets.
5.  Switch playlists safely.
6.  Loop playback.

Playback rules:

-   Ordered rotation.
-   Images display fixed time.
-   Videos play to completion.
-   Missing assets skipped.
-   Playback never blank if assets cached.

------------------------------------------------------------------------

# 10. Offline Behaviour

If network lost:

-   Playback continues.
-   Manifest polling backs off.
-   Sync resumes when online.

Fallback screen only used if no playable content exists.

------------------------------------------------------------------------

# 11. Player Cache Strategy

Devices cache:

-   Current manifest
-   Previous manifest
-   Media assets
-   Cache metadata

## 11.1 Cache Size Budget

Default cache budget: **2 GB**.

This value is chosen to balance storage availability across Fire TV
device tiers:

  Device Tier             Approx. Usable Storage   Recommended Budget
  ----------------------- ------------------------ --------------------
  Fire TV Stick Lite      ~5.5 GB                  1 GB
  Fire TV Stick           ~5.5 GB                  2 GB
  Fire TV Stick 4K        ~5.5 GB                  2 GB
  Fire TV Cube            ~10 GB                   4 GB

The cache budget is configurable per device via the `Device` record.
The manifest resolver may include a `cacheBudgetBytes` hint in
settings in a future revision.

At 2 GB, a device can typically cache:

-   ~80 images at 2 MB each, or
-   ~20 videos at 30 seconds / 5 Mbps each, or
-   A mixed playlist of ~40 items.

## 11.2 Eviction Rules

-   Protect assets referenced by current and previous manifests.
-   LRU eviction for assets not referenced by any active manifest.
-   Eviction runs when used storage exceeds 90% of budget.
-   If eviction cannot free enough space, downloads are paused until
    space becomes available (e.g., after a manifest change removes
    items).

------------------------------------------------------------------------

# 12. Manifest Resolver

Backend determines:

1.  Venue schedule.
2.  Device group membership.
3.  Winning playlist.
4.  Validity duration.

Manifest includes:

-   Playlist items
-   Validity window
-   Poll interval
-   Manifest version

ETag supports conditional updates.

------------------------------------------------------------------------

# 13. Upload & Processing Pipeline

Flow:

1.  Client requests upload session.
2.  File uploaded directly to storage.
3.  Backend validates media.
4.  Metadata extracted.
5.  Thumbnails generated.
6.  Asset marked READY.

Checksum stored for cache validation.

------------------------------------------------------------------------

## 13.1 Asset Status Transitions

    UPLOADING → PROCESSING → READY
                           → FAILED

-   `UPLOADING` — upload session created, file transfer in progress or
    awaiting completion callback.
-   `PROCESSING` — file received, backend worker is validating,
    extracting metadata, and generating thumbnails.
-   `READY` — processing complete, asset is usable in playlists.
-   `FAILED` — processing encountered an unrecoverable error.

------------------------------------------------------------------------

## 13.2 Processing Failure Handling

When processing fails, the asset record is updated with failure
details.

Asset record on failure:

``` json
{
  "assetId": "ast_501",
  "status": "FAILED",
  "failureReason": "INVALID_CODEC",
  "failureDetail": "Video uses VP9 codec. Only H.264 is supported.",
  "failedAtUtc": "2025-06-15T14:02:30Z"
}
```

Defined failure reasons:

  Reason                  Description
  ----------------------- -----------------------------------------------
  INVALID_FORMAT          File is not a supported container (JPEG, PNG,
                          MP4).
  INVALID_CODEC           Video codec is not H.264, or audio codec is not
                          AAC.
  RESOLUTION_EXCEEDED     Image or video resolution exceeds 1920x1080.
  DURATION_EXCEEDED       Video duration exceeds 120 seconds.
  BITRATE_EXCEEDED        Video bitrate exceeds 10 Mbps.
  FILE_SIZE_EXCEEDED      File exceeds maximum allowed size.
  FILE_CORRUPT            File could not be parsed or is truncated.
  CHECKSUM_MISMATCH       Stored file does not match the upload checksum.
  THUMBNAIL_FAILED        Media is valid but thumbnail generation failed.
  PROCESSING_TIMEOUT      Processing did not complete within the allowed
                          window.
  INTERNAL_ERROR          Unexpected processing failure.

The `failureDetail` field provides a human-readable explanation
suitable for display in the mobile app.

------------------------------------------------------------------------

## 13.3 Client Polling for Status

After calling `POST /v1/assets/uploads/{uploadId}/complete`, the
mobile app polls the asset record to track progress:

`GET /v1/assets/{assetId}`

Response includes `status`, and when FAILED, includes
`failureReason` and `failureDetail`.

Recommended poll interval: 2 seconds, maximum 30 attempts.

If the asset remains in UPLOADING or PROCESSING beyond the client
poll window, the app should display a message indicating processing
is still in progress and allow the user to navigate away and check
back later.

------------------------------------------------------------------------

## 13.4 Retry Policy

FAILED assets cannot be retried. The user must create a new upload
session.

Rationale: the original uploaded file may be invalid or corrupt, and
re-processing the same file would produce the same failure. A new
upload session ensures a clean file transfer.

The mobile app should:

1.  Display the failure reason from `failureDetail`.
2.  Offer a "Try Again" action that starts a new upload session
    (not a re-process of the failed asset).
3.  The failed asset record remains for audit purposes.

------------------------------------------------------------------------

## 13.5 Timeout & Cleanup Rules

Automatic timeout thresholds:

  Status       Timeout       Action
  ------------ ------------- -----------------------------------------
  UPLOADING    60 minutes    Asset marked FAILED with reason
                             PROCESSING_TIMEOUT. Uploaded file deleted.
  PROCESSING   10 minutes    Asset marked FAILED with reason
                             PROCESSING_TIMEOUT.

Orphan cleanup (background worker):

-   FAILED assets: retained for 30 days, then hard-deleted along
    with any associated storage files.
-   UPLOADING assets that were never completed: cleaned up after
    the timeout above.
-   Unused upload sessions (no file received): expired after 60
    minutes and deleted.

Storage cleanup runs as a scheduled background job (recommended:
hourly).

------------------------------------------------------------------------

# 14. Media Constraints

## Images

-   JPEG or PNG
-   Target resolution: 1920×1080
-   Recommended ≤2 MB

## Video

-   MP4 container
-   H.264 codec
-   AAC audio
-   ≤1080p resolution
-   ≤30 FPS
-   ≤10 Mbps bitrate
-   ≤120 seconds duration

------------------------------------------------------------------------

# 15. Publishing Workflow

Both playlists and schedules use:

Draft → Publish workflow.

Publish actions:

-   Copy draft to published
-   Increment version
-   Increment tenant epoch
-   Devices refresh automatically

Rollback supported via publish history.

------------------------------------------------------------------------

# 16. Concurrency Control

Uses:

-   ETag headers
-   If-Match required
-   412 responses on mismatch

Presence indicators show active editors.

------------------------------------------------------------------------

# 17. Telemetry & Audit

Devices report:

## Heartbeats

Include:

-   Playback state
-   Cache stats
-   Errors summary
-   Network status

## Event batches

Include:

-   Download failures
-   Playback failures
-   Offline transitions

Events buffered locally until upload succeeds.

------------------------------------------------------------------------

# 18. Remote Support Commands

Commands include:

-   Refresh manifest
-   Restart playback
-   Clear unused cache
-   Clear all cache
-   Restart app
-   Enable diagnostics

Devices poll commands and acknowledge results.

------------------------------------------------------------------------

# 19. Device Operating Modes

## Shared Mode (default)

-   Launch on boot
-   User may exit app
-   Device usable normally

## Kiosk Mode

-   Auto relaunch
-   App foreground enforcement
-   Screen kept awake

Configurable per device.

------------------------------------------------------------------------

# 20. Distribution Strategy

## Fire TV Player

-   MVP: sideload distribution
-   Later: Amazon Appstore

## Mobile Manager

-   TestFlight & Play internal initially
-   Later public stores

Devices report app versions for monitoring.

------------------------------------------------------------------------

# 21. Admin & Operations Console

Provides:

Dashboard: - Device status - Playback health - Alerts

Device management: - Device detail - Commands - Configuration

Content management: - Asset library - Playlists - Schedules

Publishing: - History - Rollback

Alerts: - Offline devices - Error trends

------------------------------------------------------------------------

# 22. API Status Codes

Success:

-   200 OK
-   201 Created
-   204 No Content
-   304 Not Modified

Errors:

-   400 Bad Request
-   401 Unauthorized
-   403 Forbidden
-   404 Not Found
-   409 Conflict
-   412 Precondition Failed
-   422 Rule violation
-   429 Rate limited
-   503 Service unavailable

Error responses use problem+json format.

------------------------------------------------------------------------

# 23. Development Milestones

Milestone 0: Tenant, venue, device pairing.

Milestone 1: Playback MVP with caching.

Milestone 2: Scheduling & operations tools.

Milestone 3: Operational hardening.

Milestone 4: SaaS scaling features.

------------------------------------------------------------------------

# 24. Guiding Design Principles

-   Screens must never go blank.
-   Publishing must be safe.
-   Offline playback must work.
-   Tenant isolation mandatory.
-   Devices recover automatically.
-   Operations must be simple.

------------------------------------------------------------------------

# Specification Status

This document captures **all design decisions to date**.

Sections covered:

-   System overview and logical architecture (1--4)
-   Roles, content model, scheduling, playback, offline behaviour
    (5--12)
-   Upload pipeline, media constraints, publishing, concurrency
    (13--16)
-   Telemetry, remote commands, device modes, distribution (17--20)
-   Operations console and API status codes (21--22)
-   Milestones and design principles (23--24)
-   Authentication and authorization (25)
-   Database schema (26)
-   API contracts (27)
-   Player state machine (28)
-   Backend service architecture (29)
-   Timezone handling (30)
-   Schedule resolver algorithm (31)
-   Technology stack guidance (32)
-   Player internal architecture (33)
-   Deployment and scaling architecture (34)

Further expansion may include:

-   Testing specifications
-   Operations runbooks
-   Detailed observability and alerting rules

------------------------------------------------------------------------

# 25. Authentication & Authorization

This section defines how users and devices authenticate with the
platform and how authorization decisions are enforced across tenants and
venues.

The system uses token-based authentication with strict tenant isolation.

------------------------------------------------------------------------

## 25.1 Identity Model

Two actor types authenticate:

### 1. Human Users

Users access the system through the Mobile Manager app or operations
interfaces.

Users belong to a **Tenant** and may have access to one or more venues.

### 2. Devices (Fire TV players)

Devices authenticate as headless clients using device tokens issued
during pairing.

Devices are permanently assigned to a venue unless reassigned by an
administrator.

------------------------------------------------------------------------

## 25.2 Authentication Mechanisms

### User Authentication

Users authenticate using standard identity flows:

Supported options: - Email + password - OAuth providers (optional
future) - Enterprise SSO (future)

Authentication returns:

-   Access token (JWT)
-   Refresh token
-   User profile info
-   Accessible venues

Access tokens are short-lived. Refresh tokens are longer-lived and
stored securely.

------------------------------------------------------------------------

### Device Authentication

Devices authenticate via a device token issued after pairing.

Flow:

1.  Device generates pairing code.
2.  User claims device via mobile app.
3.  Backend issues device token.
4.  Device stores token locally.

Device tokens authenticate only player endpoints.

------------------------------------------------------------------------

## 25.3 Token Structure

### User Access Token Claims

Required claims:

  Claim     Meaning
  --------- ------------------
  sub       User ID
  tid       Tenant ID
  t_roles   Tenant roles
  v_roles   Venue roles
  cap       Capability flags
  exp       Expiration
  iss       Issuer
  aud       Audience

Example:

``` json
{
  "sub": "usr_01",
  "tid": "ten_01",
  "t_roles": ["TENANT_ADMIN"],
  "v_roles": [
    {"vid": "ven_01", "role": "VENUE_MANAGER"}
  ],
  "cap": ["EDIT_SHARED_LIBRARY"],
  "exp": 1760000000
}
```

If users have access to many venues, venue roles may be resolved
server-side instead of in-token.

------------------------------------------------------------------------

### Device Token Claims

Device tokens contain:

  Claim   Meaning
  ------- ------------
  typ     device
  did     Device ID
  vid     Venue ID
  tid     Tenant ID
  exp     Expiration

Example:

``` json
{
  "typ": "device",
  "did": "dev_bar_01",
  "vid": "ven_01",
  "tid": "ten_01"
}
```

------------------------------------------------------------------------

## 25.4 Authorization Model

Authorization is evaluated using:

1.  Tenant membership
2.  Venue role assignment
3.  Capability flags
4.  Endpoint scope

### Tenant Roles

-   TENANT_ADMIN
-   TENANT_VIEWER

Tenant admins bypass venue restrictions.

------------------------------------------------------------------------

### Venue Roles

Assigned per venue:

-   VENUE_MANAGER
-   VENUE_EDITOR
-   VENUE_VIEWER

Role hierarchy:

    VIEWER < EDITOR < MANAGER

------------------------------------------------------------------------

### Capability Flags

Optional flags allow fine-grained permissions.

Example:

-   EDIT_SHARED_LIBRARY

Allows modifying tenant-wide content.

------------------------------------------------------------------------

## 25.5 Authorization Rules

Every request must satisfy:

1.  Authenticated actor
2.  Token tenant matches resource tenant
3.  Venue access allowed (if venue scoped)
4.  Required role/capability satisfied

------------------------------------------------------------------------

### Venue-scoped Authorization

For venue endpoints:

    User must have role for venue
    OR be tenant admin

If resource belongs to another tenant, system returns **404 Not Found**.

------------------------------------------------------------------------

### Device Authorization

Devices may only:

-   Access their own venue
-   Fetch manifests
-   Send telemetry
-   Receive commands

Devices cannot access management endpoints.

------------------------------------------------------------------------

## 25.6 Tenant Isolation Enforcement

Tenant isolation is mandatory.

Rules:

-   Queries must always filter by tenant ID.
-   Venue must belong to tenant.
-   Devices must belong to venue.
-   Cross-tenant access returns 404.

No endpoint accepts tenant IDs from clients without verification.

------------------------------------------------------------------------

## 25.7 Token Lifecycle

### User Tokens

Access token lifetime: short (e.g., 15--60 min) Refresh token lifetime:
long (days/weeks)

Refresh flow: - Client exchanges refresh token for new access token.

------------------------------------------------------------------------

### Device Tokens

Device tokens may: - Be long-lived OR - Use rotating tokens with renewal

Device token invalidation occurs when: - Device removed - Venue
reassigned - Tenant disabled

------------------------------------------------------------------------

## 25.8 Session Security

Clients must:

-   Store tokens securely
-   Avoid logging tokens
-   Use HTTPS only
-   Prevent token leakage

Devices store tokens in secure app storage.

------------------------------------------------------------------------

## 25.9 Audit Logging

Authentication and authorization events recorded:

-   Login attempts
-   Role changes
-   Device pairing
-   Command execution
-   Publish actions

Audit logs are tenant-scoped.

------------------------------------------------------------------------

## 25.10 Failure Handling

Authentication failures:

  Status   Reason
  -------- ----------------------------
  401      Invalid or expired token
  403      Permission denied
  404      Resource hidden by tenancy

Devices receiving 401 should retry once then require re-pairing.

------------------------------------------------------------------------

## 25.11 Future Extensions

Planned improvements:

-   SSO integration
-   Multi-factor authentication
-   Enterprise identity federation
-   Temporary support access tokens
-   API key access for integrations

------------------------------------------------------------------------

------------------------------------------------------------------------

# 26. Database Schema (Multi-Tenant / Multi-Venue)

This section defines the core relational schema for the platform. It is
designed for strict tenant isolation, venue-scoped operations, and
efficient manifest resolution.

## 26.1 Design Goals

-   Hard tenant isolation: every row is tenant-scoped directly or via
    venue foreign keys.
-   Venue-scoped schedules and devices.
-   Tenant-shared and venue-only content libraries.
-   Draft/Published versioning with immutable published snapshots
    (supports rollback).
-   Optimistic concurrency via integer versions and ETags.
-   Efficient reads for player manifest resolution and admin dashboards.

------------------------------------------------------------------------

## 26.2 Key Concepts & Identifiers

Recommended identifier types: - `UUID` for external IDs (tenantId,
venueId, userId, etc.). - `BIGINT` for internal auto-increment keys
(optional) for performance. - Timestamps stored as UTC.

Conventions: - All tables include: - `createdAtUtc`, `updatedAtUtc` -
`createdByUserId` where relevant - Soft delete (optional):
`deletedAtUtc` on user-managed resources.

------------------------------------------------------------------------

## 26.3 Core Tables

### Tenant

-   tenantId (PK)
-   name
-   status
-   defaultTimezone
-   createdAtUtc
-   updatedAtUtc

------------------------------------------------------------------------

### Venue

-   venueId (PK)
-   tenantId (FK)
-   name
-   timezone
-   address fields
-   fallbackPlaylistId
-   createdAtUtc
-   updatedAtUtc

------------------------------------------------------------------------

### User

-   userId (PK)
-   tenantId (FK)
-   email
-   displayName
-   status
-   createdAtUtc
-   updatedAtUtc

------------------------------------------------------------------------

### TenantUserRole

-   tenantUserRoleId (PK)
-   tenantId
-   userId
-   role
-   timestamps

------------------------------------------------------------------------

### VenueUserRole

-   venueUserRoleId (PK)
-   tenantId
-   venueId
-   userId
-   role
-   timestamps

------------------------------------------------------------------------

## 26.4 Devices & Grouping

### Device

-   deviceId (PK)
-   tenantId
-   venueId
-   name
-   primaryGroupId
-   timezone
-   runMode
-   launchOnBoot
-   autoRelaunch
-   keepAwake
-   watchdogReturnToForeground
-   orientation
-   status
-   lastSeenAtUtc
-   appVersion
-   timestamps

------------------------------------------------------------------------

### DeviceGroup

-   groupId (PK)
-   tenantId
-   venueId
-   name
-   timestamps

------------------------------------------------------------------------

## 26.5 Pairing & Device Tokens

### DevicePairing

-   pairingId (PK)
-   pairingCode
-   deviceHardwareId
-   status (PENDING, CLAIMED, COMPLETED, EXPIRED)
-   expiresAtUtc
-   claimedByUserId
-   claimedTenantId
-   claimedVenueId
-   resultingDeviceId
-   timestamps

Notes:

-   `deviceHardwareId` is a device-generated identifier included in
    the QR code so the backend can correlate the pairing request back
    to the physical device.
-   `claimedTenantId`, `claimedVenueId`, and `claimedByUserId` are
    populated when a user claims the device via the mobile app.
-   `resultingDeviceId` is set when pairing completes and the Device
    record is created.
-   Status transitions: PENDING → CLAIMED → COMPLETED, or
    PENDING → EXPIRED.

------------------------------------------------------------------------

### DeviceAuthToken (optional)

-   deviceAuthTokenId (PK)
-   tenantId
-   venueId
-   deviceId
-   tokenHash
-   status
-   issuedAtUtc
-   timestamps

------------------------------------------------------------------------

## 26.6 Assets

### Asset

-   assetId (PK)
-   tenantId
-   scopeType
-   scopeVenueId
-   type
-   status
-   displayName
-   file metadata
-   checksumSha256
-   media metadata
-   cdnUrl
-   thumbnailUrl
-   timestamps
-   deletedAtUtc

------------------------------------------------------------------------

## 26.7 Playlists

### Playlist

-   playlistId (PK)
-   tenantId
-   scopeType
-   scopeVenueId
-   name
-   draftVersion
-   publishedVersion
-   timestamps
-   deletedAtUtc

------------------------------------------------------------------------

### PlaylistItem

-   playlistItemId (PK)
-   playlistId
-   assetId
-   sequence
-   durationSeconds
-   enabled
-   timestamps

------------------------------------------------------------------------

### PlaylistPublished

-   playlistPublishedId (PK)
-   playlistId
-   publishedVersion
-   publishedAtUtc
-   publishedByUserId

------------------------------------------------------------------------

### PlaylistPublishedItem

-   playlistPublishedItemId (PK)
-   playlistId
-   publishedVersion
-   assetId
-   sequence
-   durationSeconds

------------------------------------------------------------------------

## 26.8 Venue Scheduling

### VenueScheduleSet

-   venueScheduleSetId (PK)
-   tenantId
-   venueId
-   draftVersion
-   publishedVersion
-   timestamps

------------------------------------------------------------------------

### ScheduleSlot

-   slotId (PK)
-   tenantId
-   venueId
-   targetType
-   targetId
-   playlistId
-   daysOfWeekMask
-   startMinutes (0--1439, minutes since midnight in venue local time)
-   endMinutes (0--1439, minutes since midnight in venue local time)
-   priority
-   validFromDate
-   validToDate
-   enabled
-   timestamps

------------------------------------------------------------------------

### SchedulePublished

-   schedulePublishedId (PK)
-   venueId
-   publishedVersion
-   publishedAtUtc
-   publishedByUserId

------------------------------------------------------------------------

### SchedulePublishedSlot

-   schedulePublishedSlotId (PK)
-   tenantId
-   venueId
-   publishedVersion
-   targetType
-   targetId
-   playlistId
-   daysOfWeekMask
-   startMinutes
-   endMinutes
-   priority
-   validFromDate
-   validToDate
-   enabled
-   timestamps

------------------------------------------------------------------------

## 26.9 Venue Manifest Epoch

### VenueEpoch

-   venueId (PK)
-   tenantId
-   manifestEpoch
-   updatedAtUtc

Epoch increments whenever playback-affecting data changes.

------------------------------------------------------------------------

## 26.10 Telemetry & Commands

### DeviceStatus

-   deviceId (PK)
-   venueId
-   lastHeartbeatAtUtc
-   isOnline
-   playback & cache stats
-   updatedAtUtc

------------------------------------------------------------------------

### DeviceEvent

-   deviceEventId (PK)
-   deviceId
-   eventType
-   eventTimeUtc
-   payloadJson
-   severity
-   createdAtUtc

------------------------------------------------------------------------

### DeviceCommand

-   commandId (PK)
-   deviceId
-   type
-   parametersJson
-   status
-   issuedByUserId
-   timestamps
-   resultJson
-   error fields

------------------------------------------------------------------------

## 26.11 Publishing History

### PublishEvent

-   publishEventId (PK)
-   tenantId
-   venueId
-   type
-   entityId
-   version fields
-   publishedByUserId
-   publishedAtUtc

------------------------------------------------------------------------

## 26.12 Presence

### EditorPresence

-   presenceId (PK)
-   tenantId
-   resourceType
-   resourceId
-   userId
-   expiresAtUtc

------------------------------------------------------------------------

End of Database Schema section.

------------------------------------------------------------------------

# 27. API Contract (v1)

This section defines the HTTP API surface for the platform, including
route structure, authentication requirements, status codes, concurrency
requirements (ETag/If-Match), and core request/response shapes.

All endpoints are prefixed with `/v1`.

## 27.1 Conventions

### Authentication

-   **User endpoints** require a **User Access Token** (JWT) in:
    -   `Authorization: Bearer <token>`
-   **Player endpoints** require a **Device Token** in the same header.

### Content Types

-   Requests: `application/json` (unless uploading direct-to-storage)
-   Error responses: `application/problem+json`

### Correlation

-   All responses include: `X-Request-Id: <uuid>`

### Pagination (for list endpoints)

Use cursor pagination. - Query: `?cursor=<token>&limit=<n>` - Default
`limit=50`, max `limit=200`

Response:

``` json
{
  "items": [ ... ],
  "nextCursor": "..."
}
```

### Filtering

Common filter conventions: - `?q=<search>` - `?status=<value>` -
`?scope=TENANT|VENUE` - `?type=IMAGE|VIDEO`

### Concurrency / Optimistic Locking

For draft-modifying operations: - `GET` returns `ETag` - Mutations
require `If-Match` header - Missing `If-Match` -\>
`428 Precondition Required` - ETag mismatch -\>
`412 Precondition Failed`

### Rate Limiting

Rate limits protect backend services from abuse and ensure fair usage
across tenants.

Scoping:

-   **User endpoints** — rate limited per tenant, per user.
-   **Player endpoints** — rate limited per device.
-   **Authentication endpoints** — rate limited per IP address and
    per account.

Default limits:

  Endpoint Category            Scope        Limit
  ---------------------------- ------------ --------------------
  POST /v1/auth/login          per IP       10 requests / minute
  POST /v1/auth/login          per account  5 requests / minute
  POST /v1/auth/refresh        per user     30 requests / hour
  Library & venue mutations    per tenant   120 requests / minute
  Publish operations           per tenant   10 requests / minute
  Upload sessions              per tenant   30 requests / hour
  GET /v1/player/manifest      per device   2 requests / minute
  POST /v1/player/heartbeat    per device   2 requests / minute
  POST /v1/player/events       per device   6 requests / minute
  GET /v1/player/commands      per device   2 requests / minute

Response headers on all requests:

-   `X-RateLimit-Limit` — maximum requests in the current window.
-   `X-RateLimit-Remaining` — requests remaining in the current
    window.
-   `X-RateLimit-Reset` — UTC epoch seconds when the window resets.

When rate limited (429 Too Many Requests):

-   Response includes `Retry-After` header (seconds until next
    allowed request).
-   Response body uses standard error format (Section 27.2) with
    `retryAfterSeconds` field.

Player behaviour on 429: back off using the `Retry-After` value,
falling back to exponential backoff if the header is missing.

Limits are tuneable per tenant for enterprise customers.

------------------------------------------------------------------------

### Common Status Codes

-   200 OK, 201 Created, 204 No Content, 304 Not Modified
-   400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
-   409 Conflict, 412 Precondition Failed, 422 Unprocessable Entity
-   428 Precondition Required, 429 Too Many Requests, 503 Service
    Unavailable

------------------------------------------------------------------------

## 27.2 Error Payload Format

All error responses use:

``` json
{
  "type": "https://api.yourdomain/errors/PRECONDITION_FAILED",
  "title": "Precondition Failed",
  "status": 412,
  "code": "PRECONDITION_FAILED",
  "detail": "Resource has changed since it was loaded.",
  "instance": "/v1/library/playlists/pl_123/reorder",
  "requestId": "req_..."
}
```

Optional fields: - `errors` - `currentEtag` - `retryAfterSeconds` -
`context`

------------------------------------------------------------------------

# 27.3 Authentication Endpoints

## User Login

`POST /v1/auth/login`

200:

``` json
{
  "accessToken": "...",
  "refreshToken": "...",
  "user": { "userId": "usr_1", "tenantId": "ten_1" }
}
```

------------------------------------------------------------------------

## Refresh Token

`POST /v1/auth/refresh`

200 returns new tokens.

------------------------------------------------------------------------

## Current User

`GET /v1/me`

Returns profile and roles.

------------------------------------------------------------------------

## Accessible Venues

`GET /v1/me/venues`

Returns venues user may access.

------------------------------------------------------------------------

# 27.4 Tenant Admin Endpoints

Endpoints:

-   `POST /v1/tenants/{tenantId}/venues`
-   `GET /v1/tenants/{tenantId}/venues`
-   `GET /v1/tenants/{tenantId}/venues/{venueId}`
-   `PUT /v1/tenants/{tenantId}/venues/{venueId}`
-   `POST /v1/tenants/{tenantId}/users`
-   `GET /v1/tenants/{tenantId}/users`
-   `GET /v1/tenants/{tenantId}/users/{userId}`
-   `PUT /v1/tenants/{tenantId}/users/{userId}/venue-roles`

Tenant admin permissions required.

------------------------------------------------------------------------

# 27.5 Content Library Endpoints

Shared (tenant-scoped) content management:

Assets:

-   `GET /v1/library/assets`
-   `POST /v1/library/assets/uploads`
-   `POST /v1/library/assets/uploads/{uploadId}/complete`
-   `GET /v1/library/assets/{assetId}`
-   `DELETE /v1/library/assets/{assetId}`

Playlists:

-   `GET /v1/library/playlists`
-   `POST /v1/library/playlists`
-   `GET /v1/library/playlists/{playlistId}`
-   `PUT /v1/library/playlists/{playlistId}/items`
-   `PUT /v1/library/playlists/{playlistId}/reorder`
-   `POST /v1/library/playlists/{playlistId}/publish`

Presence endpoints support editing coordination.

------------------------------------------------------------------------

# 27.6 Venue Operations

Venue-scoped endpoints:

Devices: - `GET /v1/venues/{venueId}/devices` -
`GET /v1/venues/{venueId}/devices/{deviceId}` -
`PATCH /v1/venues/{venueId}/devices/{deviceId}`

Groups: - CRUD endpoints under `/groups`

Venue content:

-   `GET /v1/venues/{venueId}/assets`
-   `POST /v1/venues/{venueId}/assets/uploads`
-   `POST /v1/venues/{venueId}/assets/uploads/{uploadId}/complete`
-   `GET /v1/venues/{venueId}/assets/{assetId}`
-   `DELETE /v1/venues/{venueId}/assets/{assetId}`
-   `GET /v1/venues/{venueId}/playlists`
-   `POST /v1/venues/{venueId}/playlists`
-   `GET /v1/venues/{venueId}/playlists/{playlistId}`
-   `PUT /v1/venues/{venueId}/playlists/{playlistId}/items`
-   `PUT /v1/venues/{venueId}/playlists/{playlistId}/reorder`
-   `POST /v1/venues/{venueId}/playlists/{playlistId}/publish`

Scheduling: - `GET /v1/venues/{venueId}/scheduleset` -
`POST /v1/venues/{venueId}/scheduleset/slots` -
`PUT /v1/venues/{venueId}/scheduleset/slots/{slotId}` -
`DELETE /v1/venues/{venueId}/scheduleset/slots/{slotId}` -
`POST /v1/venues/{venueId}/scheduleset/publish`

Publishing history: - `GET /v1/venues/{venueId}/publishes`

Remote commands: -
`POST /v1/venues/{venueId}/devices/{deviceId}/commands`

------------------------------------------------------------------------

# 27.7 Pairing Endpoints

Device: - `POST /v1/devices/pairing/start` -
`GET /v1/devices/pairing/status`

User: - `POST /v1/venues/{venueId}/devices/pairing/claim`

------------------------------------------------------------------------

# 27.8 Player Endpoints

Require device authentication.

-   `GET /v1/player/manifest`
-   `POST /v1/player/heartbeat`
-   `POST /v1/player/events`
-   `GET /v1/player/commands`
-   `POST /v1/player/commands/{commandId}/result`

------------------------------------------------------------------------

## 27.8.1 Manifest Response Schema

`GET /v1/player/manifest`

Headers:

-   `ETag: "<manifestEtag>"` — used by player for conditional requests
    via `If-None-Match`
-   `304 Not Modified` returned when ETag matches (no body)

200 Response:

``` json
{
  "manifestEtag": "e:ven_01:47",
  "validToUtc": "2025-06-15T14:00:00Z",
  "nextCheckSeconds": 60,

  "settings": {
    "runMode": "KIOSK",
    "launchOnBoot": true,
    "autoRelaunch": true,
    "keepAwake": true,
    "watchdogReturnToForeground": true,
    "orientation": "LANDSCAPE"
  },

  "playlist": {
    "playlistId": "pl_042",
    "publishedVersion": 3,
    "items": [
      {
        "assetId": "ast_101",
        "type": "IMAGE",
        "url": "https://cdn.example.com/ten_01/ast_101.jpg",
        "checksumSha256": "a1b2c3...",
        "durationSeconds": 10,
        "widthPx": 1920,
        "heightPx": 1080,
        "fileSizeBytes": 245000
      },
      {
        "assetId": "ast_202",
        "type": "VIDEO",
        "url": "https://cdn.example.com/ten_01/ast_202.mp4",
        "checksumSha256": "d4e5f6...",
        "durationSeconds": 30,
        "widthPx": 1920,
        "heightPx": 1080,
        "fileSizeBytes": 8500000
      }
    ]
  }
}
```

Field definitions:

  Field                                Type       Description
  ------------------------------------ ---------- -----------------------------------------------
  manifestEtag                         string     Opaque version tag. Player sends as
                                                  If-None-Match on next poll.
  validToUtc                           ISO 8601   When this manifest expires. Player should
                                                  refresh before this time.
  nextCheckSeconds                     integer    Suggested poll interval in seconds.
  settings.runMode                     enum       KIOSK or SHARED.
  settings.launchOnBoot                boolean    Whether app starts on device boot.
  settings.autoRelaunch                boolean    Whether app relaunches if exited (kiosk).
  settings.keepAwake                   boolean    Whether screen sleep is prevented.
  settings.watchdogReturnToForeground  boolean    Whether app forces itself to foreground.
  settings.orientation                 enum       LANDSCAPE or PORTRAIT.
  playlist.playlistId                  string     Published playlist identifier.
  playlist.publishedVersion            integer    Snapshot version for change detection.
  playlist.items[].assetId             string     Asset identifier. Used as cache key.
  playlist.items[].type                enum       IMAGE or VIDEO.
  playlist.items[].url                 string     CDN download URL for the asset.
  playlist.items[].checksumSha256      string     Hex-encoded SHA-256. Player verifies after
                                                  download.
  playlist.items[].durationSeconds     integer    Display duration for images. Videos play to
                                                  completion regardless.
  playlist.items[].widthPx             integer    Asset width in pixels.
  playlist.items[].heightPx            integer    Asset height in pixels.
  playlist.items[].fileSizeBytes       integer    File size. Allows player to estimate download
                                                  time and manage cache budget.

No-playlist response (when resolver finds no matching schedule and no
fallback):

``` json
{
  "manifestEtag": "e:ven_01:47",
  "validToUtc": "2025-06-15T14:00:00Z",
  "nextCheckSeconds": 60,
  "settings": { "..." : "..." },
  "playlist": null
}
```

Player behaviour: when `playlist` is `null` and no cached content
exists, display ERROR_SCREEN per Section 28.4.

------------------------------------------------------------------------

## 27.8.2 Heartbeat Request Schema

`POST /v1/player/heartbeat`

Sent periodically by the player to report current device state.
Backend updates the DeviceStatus record and derives online/offline
status.

Request:

``` json
{
  "deviceTime": "2025-06-15T13:45:12Z",
  "appVersion": "1.2.0",
  "state": "PLAYING",
  "manifestEtag": "e:ven_01:47",
  "playback": {
    "playlistId": "pl_042",
    "publishedVersion": 3,
    "currentAssetId": "ast_101",
    "currentItemIndex": 0,
    "loopCount": 14
  },
  "cache": {
    "totalBytes": 2147483648,
    "usedBytes": 524288000,
    "assetCount": 12,
    "pendingDownloads": 1
  },
  "network": {
    "connected": true,
    "type": "ETHERNET",
    "lastSyncAtUtc": "2025-06-15T13:44:55Z"
  },
  "errors": {
    "playbackFailures": 0,
    "downloadFailures": 2,
    "manifestFetchFailures": 0
  }
}
```

Field definitions:

  Field                       Type       Description
  --------------------------- ---------- -----------------------------------------------
  deviceTime                  ISO 8601   Device local clock in UTC. Server receive time
                                         is authoritative if device clock is wrong.
  appVersion                  string     Player app version string.
  state                       enum       Current player state: PLAYING, ERROR_SCREEN,
                                         PAIRING, BOOTING.
  manifestEtag                string     ETag of the currently active manifest. Null if
                                         no manifest loaded.
  playback.playlistId         string     Currently playing playlist. Null if not
                                         playing.
  playback.publishedVersion   integer    Playlist snapshot version in use.
  playback.currentAssetId     string     Asset currently on screen.
  playback.currentItemIndex   integer    Zero-based index in playlist.
  playback.loopCount          integer    Number of complete playlist loops since last
                                         manifest apply.
  cache.totalBytes            long       Total cache storage budget in bytes.
  cache.usedBytes             long       Currently used cache storage in bytes.
  cache.assetCount            integer    Number of cached assets.
  cache.pendingDownloads      integer    Assets queued for download.
  network.connected           boolean    Whether network is currently reachable.
  network.type                enum       ETHERNET, WIFI, or NONE.
  network.lastSyncAtUtc       ISO 8601   Last successful backend communication.
  errors.playbackFailures     integer    Playback errors since last heartbeat.
  errors.downloadFailures     integer    Download errors since last heartbeat.
  errors.manifestFetchFailures integer   Manifest fetch errors since last heartbeat.

Response: `204 No Content`

Heartbeat interval: aligned with `nextCheckSeconds` from the manifest
(typically 60 seconds).

------------------------------------------------------------------------

## 27.8.3 Event Batch Request Schema

`POST /v1/player/events`

Sends a batch of discrete events. Events are buffered locally on the
device and flushed periodically or when a batch threshold is reached.

Request:

``` json
{
  "deviceTime": "2025-06-15T13:45:30Z",
  "events": [
    {
      "eventType": "PLAYBACK_FAILED",
      "eventTimeUtc": "2025-06-15T13:42:10Z",
      "severity": "ERROR",
      "payload": {
        "assetId": "ast_303",
        "reason": "DECODE_ERROR",
        "message": "MediaCodec failed to initialize H.264 decoder"
      }
    },
    {
      "eventType": "DOWNLOAD_FAILED",
      "eventTimeUtc": "2025-06-15T13:43:55Z",
      "severity": "WARNING",
      "payload": {
        "assetId": "ast_404",
        "url": "https://cdn.example.com/ten_01/ast_404.mp4",
        "httpStatus": 503,
        "retryCount": 3
      }
    }
  ]
}
```

Field definitions:

  Field                  Type       Description
  ---------------------- ---------- -----------------------------------------------
  deviceTime             ISO 8601   Device clock at time of submission.
  events[]               array      One or more events. Max 100 per batch.
  events[].eventType     enum       Event type identifier (see table below).
  events[].eventTimeUtc  ISO 8601   When the event occurred on the device.
  events[].severity      enum       INFO, WARNING, or ERROR.
  events[].payload       object     Event-specific data. Structure varies by
                                    eventType.

Defined event types:

  Event Type                     Severity   Payload Fields
  ------------------------------ ---------- ---------------------------------
  APP_STARTED                    INFO       appVersion, deviceModel, osVersion
  PLAYBACK_FAILED                ERROR      assetId, reason, message
  DOWNLOAD_FAILED                WARNING    assetId, url, httpStatus, retryCount
  MANIFEST_FETCH_FAILED          WARNING    httpStatus, reason
  MANIFEST_STAGED                INFO       manifestEtag
  MANIFEST_APPLIED               INFO       manifestEtag, previousEtag
  MANIFEST_REJECTED              WARNING    manifestEtag, reason
  OFFLINE_ENTERED                WARNING    lastOnlineAtUtc
  ONLINE_RESTORED                INFO       offlineDurationSeconds
  FALLBACK_TO_PREVIOUS_MANIFEST  WARNING    currentEtag, fallbackEtag
  NO_PLAYABLE_CONTENT            ERROR      manifestEtag
  ERROR_SCREEN_ENTERED           ERROR      reason
  LOCAL_MANIFEST_CORRUPT         ERROR      fileName, parseError
  COMMAND_EXECUTED               INFO       commandId, commandType
  COMMAND_FAILED                 ERROR      commandId, commandType, reason
  DEVICE_TOKEN_INVALID           ERROR      httpStatus
  CACHE_EVICTION                 INFO       assetId, freedBytes, reason

Response: `204 No Content`

Batch constraints:

-   Maximum 100 events per request.
-   If the local buffer exceeds 100 events, the player splits across
    multiple requests.
-   Events older than 7 days may be discarded by the player to
    prevent unbounded local storage growth.
-   On repeated submission failure, the player backs off using
    exponential backoff matching the manifest sync loop.

------------------------------------------------------------------------

# 27.9 Rollback Endpoints

Optional admin features:

-   `POST /v1/library/playlists/{playlistId}/rollback`
-   `POST /v1/venues/{venueId}/scheduleset/rollback`

------------------------------------------------------------------------

# 27.10 Validation Rules

Key enforcement rules:

-   Playlist publish requires READY assets.
-   Schedule publish requires published playlists.
-   Venue access validated per tenant.
-   Unauthorized tenant access returns 404.

------------------------------------------------------------------------

End of API Contract section.

------------------------------------------------------------------------

# 28. Player State Machine (Fire TV Player)

This section specifies the Fire TV Player runtime behavior as a formal
state machine, including transitions, error handling, offline behavior,
caching interactions, telemetry emission, and command execution rules.

## 28.1 Goals

-   Start playback fast (first frame ASAP).
-   Never blank the screen if cached playable content exists.
-   Support offline operation indefinitely (within cache limits).
-   Apply schedule/playlist updates safely (no mid-video interruptions
    in MVP).
-   Provide actionable telemetry and support command execution.
-   Remain stable under partial failures (CDN, backend, storage,
    decoding).

------------------------------------------------------------------------

## 28.2 Core Concepts

### Manifests

-   Player consumes a manifest from `GET /v1/player/manifest`.
-   Manifest contains:
    -   settings (run mode, launch on boot, keep awake, etc.)
    -   playlist (ordered items)
    -   `validToUtc`
    -   `nextCheckSeconds`
    -   `manifestEtag` (ETag)

### Two-phase apply

-   `CURRENT` manifest: active playback source.
-   `STAGED` manifest: candidate update; downloads assets first.
-   Only swap `CURRENT ← STAGED` at safe boundaries.

### Cache layers

-   `currentManifest.json`
-   `previousManifest.json`
-   Asset files + metadata DB (SQLite recommended)

------------------------------------------------------------------------

## 28.3 State Machine Overview

### Primary States

1.  `BOOT`
2.  `LOAD_LOCAL`
3.  `PLAYABLE_CHECK`
4.  `PLAYING`
5.  `SYNC_MANIFEST`
6.  `SYNC_COMMANDS`
7.  `DOWNLOAD_ASSETS`
8.  `APPLY_STAGED`
9.  `OFFLINE_BACKOFF`
10. `ERROR_SCREEN`

Note: Sync and download behaviors typically run in background workers
while PLAYING, but are described here as logical states.

------------------------------------------------------------------------

## 28.4 State Definitions

### BOOT

Entry actions: - Initialize logging. - Open cache database/index. - Load
configuration defaults. - Start crash/ANR guards where available.

Transitions: - Immediately → LOAD_LOCAL

Telemetry: - Emit `APP_STARTED` with app/device metadata.

------------------------------------------------------------------------

### LOAD_LOCAL

Entry actions: - Load current and previous manifests from local
storage. - Validate schema. - Load cached asset index.

Transitions: - → PLAYABLE_CHECK

Telemetry: - Emit `LOCAL_MANIFEST_CORRUPT` if invalid.

------------------------------------------------------------------------

### PLAYABLE_CHECK

Logic: - Determine playable assets from cache. - Prefer current
manifest; fallback to previous if necessary.

Transitions: - Playable current → PLAYING - Else playable previous →
PLAYING - Else → ERROR_SCREEN

Telemetry: - `FALLBACK_TO_PREVIOUS_MANIFEST` - `NO_PLAYABLE_CONTENT`

------------------------------------------------------------------------

### PLAYING

Entry actions: - Apply runtime settings. - Begin ordered rotation
playback.

Playback rules: - Skip missing assets. - Images show fixed duration. -
Videos play fully. - Errors skip item.

Switch rules: - No manifest swap mid-video. - Swap only at safe
boundaries.

Transitions: - Remain in PLAYING normally. - If playback impossible →
PLAYABLE_CHECK or ERROR_SCREEN.

Telemetry: - `PLAYBACK_FAILED` on decode/play errors.

------------------------------------------------------------------------

### ERROR_SCREEN

Displayed only if no playable content exists.

Entry actions: - Show fallback UI. - Continue sync attempts.

Transitions: - Content available → PLAYING

Telemetry: - Emit `ERROR_SCREEN_ENTERED`.

------------------------------------------------------------------------

## 28.5 Staging & Applying Updates

### Staged Manifest Lifecycle

States: - STAGED_NONE - STAGED_WAITING_ASSETS - STAGED_READY -
STAGED_REJECTED

Actions: - Validate schema. - Determine asset requirements. - Queue
downloads.

### Applying Staged Manifest

At safe boundary: - previous ← current - current ← staged - Persist
manifests.

Apply when: - All assets ready OR - Partial playable & current expired.

Reject if invalid or permanently unplayable.

Telemetry: - MANIFEST_STAGED - MANIFEST_APPLIED - MANIFEST_REJECTED

------------------------------------------------------------------------

## 28.6 Manifest Sync Loop

Poll cadence: - Normal: every \~60s - Near expiry: more frequent
checks - Failure: exponential backoff

Responses: - 304 → no change - 200 → stage manifest - 401 → unregistered
flow - 429/503 → backoff

Telemetry: - MANIFEST_FETCH_FAILED - ONLINE_RESTORED - OFFLINE_ENTERED

------------------------------------------------------------------------

## 28.7 Command Polling & Execution

Commands fetched periodically.

Execution at safe boundaries: - REQUEST_MANIFEST_REFRESH -
RESTART_PLAYBACK_LOOP - CLEAR_CACHE_UNUSED - CLEAR_CACHE_ALL -
REBOOT_APP - SET_LOG_LEVEL

Acknowledge via command result endpoint.

Persist last command processed.

Telemetry: - COMMAND_EXECUTED - COMMAND_FAILED

------------------------------------------------------------------------

## 28.8 Telemetry Loops

### Heartbeat

Sent periodically with playback and cache stats.

### Event batching

Buffered locally; flushed regularly.

Handles payload splitting, retry, and backoff.

------------------------------------------------------------------------

## 28.9 Cache & Download Integration

Download priority: 1. Staged assets 2. Upcoming playlist items 3.
Remaining assets

Downloads: - Use temp files - Verify checksums - Atomic move into cache

Failures marked for retry later.

------------------------------------------------------------------------

## 28.10 Unregistered / Token Invalid Behavior

On repeated 401 responses: - Clear device token - Enter pairing mode -
Display pairing screen

Telemetry: - DEVICE_TOKEN_INVALID

------------------------------------------------------------------------

## 28.11 Kiosk vs Shared Mode

Shared: - Launch on boot - User may exit

Kiosk: - Auto relaunch - Force foreground where possible - Keep awake

Settings updated from manifest.

------------------------------------------------------------------------

## 28.12 State Transition Summary

  From             Event                 To
  ---------------- --------------------- ----------------
  BOOT             Init complete         LOAD_LOCAL
  LOAD_LOCAL       Loaded                PLAYABLE_CHECK
  PLAYABLE_CHECK   Playable found        PLAYING
  PLAYABLE_CHECK   None playable         ERROR_SCREEN
  ERROR_SCREEN     Content restored      PLAYING
  PLAYING          Playback impossible   PLAYABLE_CHECK
  Any              Token invalid         UNREGISTERED

------------------------------------------------------------------------

End of Player State Machine section.

------------------------------------------------------------------------

# 29. Backend Service Architecture

This section defines the backend service architecture for the platform,
including service boundaries, responsibilities, data flows, background
processing, caching strategy, and scalability considerations.

## 29.1 Architectural Goals

-   Clear separation of responsibilities (auth, content, scheduling,
    playback, telemetry).
-   Strong tenant isolation and authorization enforcement.
-   High availability for playback-critical endpoints (manifest +
    commands).
-   Eventual consistency where acceptable; strong consistency for
    publishing.
-   Efficient media delivery via storage + CDN (backend not in the hot
    path for large files).
-   Observable operations (metrics, logs, alerts).

------------------------------------------------------------------------

## 29.2 High-Level Components

Logical components:

-   API Gateway / BFF (user endpoints)
-   Player Edge API (device endpoints)
-   Background Worker(s)
-   Relational database
-   Redis/Cache (recommended)
-   Object storage + CDN
-   Observability stack (logs/metrics/traces)

------------------------------------------------------------------------

## 29.3 Service Boundary Options

Recommended for MVP: **Modular monolith** with clear internal modules
that can be extracted later:

1.  Auth
2.  Tenant/Venue
3.  Content
4.  Playlist
5.  Scheduling
6.  Devices
7.  Playback (manifest resolver)
8.  Commands
9.  Telemetry
10. Operations/Alerts

------------------------------------------------------------------------

## 29.4 Core Modules & Responsibilities

### 29.4.1 API Gateway / BFF

-   Validates user JWT.
-   Enforces authorization.
-   Aggregates data for mobile/ops.
-   Rate limits sensitive endpoints.
-   Standardizes error responses.

### 29.4.2 Player Edge API

-   Validates device tokens.
-   Serves `/v1/player/manifest` and `/v1/player/commands` with low
    latency.
-   Accepts telemetry: heartbeat and events.
-   Uses caching to withstand polling load.

### 29.4.3 Content Module

-   Asset records and state transitions.
-   Upload sessions (pre-signed URLs).
-   Media validation and metadata extraction.
-   Thumbnail generation.
-   Optional transcoding orchestration.

### 29.4.4 Playlist Module

-   Playlist CRUD and ordering.
-   Draft editing + ETag concurrency.
-   Publish snapshot creation and rollback support.
-   Presence indicator integration.
-   Increments venue epoch when shared playlist publishes affect venues.

### 29.4.5 Scheduling Module

-   Draft schedule editing + ETag concurrency.
-   Publish schedule snapshot.
-   Validates playlist visibility and published state.
-   Increments venue epoch on publish.

### 29.4.6 Devices Module

-   Pairing code generation and claim.
-   Device registration and configuration.
-   Groups management.
-   Command creation for remote support.
-   Tracks last seen and status summaries.

### 29.4.7 Playback / Manifest Resolver Module

-   Resolves winning published playlist for a device at current time.
-   Builds manifest payload (settings + playlist items).
-   Supports ETag / 304 responses.
-   Uses venue epoch for fast invalidation.
-   Caches resolved results briefly for high throughput.

### 29.4.8 Commands Module

-   Maintains per-device command queue.
-   Supports device polling.
-   Tracks delivery and results.
-   Exposes command history to admins.

### 29.4.9 Telemetry Module

-   Ingests heartbeats (updates DeviceStatus).
-   Ingests events (append DeviceEvent).
-   Optional async ingestion pipeline when volume grows.
-   Supports retention/rollups.

### 29.4.10 Operations / Alerts Module

-   Offline detection and error pattern flagging.
-   Low disk / repeated download failures.
-   Admin views to triage device issues.
-   Optional notifications later.

------------------------------------------------------------------------

## 29.5 Background Workers

Required worker tasks:

-   Asset processing (validate, metadata, thumbnails, optional
    transcode).
-   Cleanup jobs (expire pairing codes, commands, presence records).
-   Telemetry retention/aggregation (optional).
-   Orphan cleanup for failed uploads.

------------------------------------------------------------------------

## 29.6 Caching & Performance

Redis recommended for: - Presence - Manifest short cache - Rate limiting
counters - Optional command caching

Manifest endpoint must be highly optimized: - Minimal payload - CDN URLs
in manifest - Conditional GET (304) - Cache invalidation via venue epoch
increments

------------------------------------------------------------------------

## 29.7 Data Flow Scenarios

### Upload → Publish → Playback

1.  Mobile starts upload session
2.  Upload direct to storage
3.  Worker validates and marks READY
4.  Playlist updated and published
5.  Schedule updated and published
6.  Venue epoch increments
7.  Devices poll and stage manifest updates

### Support command

1.  Admin issues command
2.  Device polls command queue
3.  Executes at safe point
4.  Posts result
5.  Admin UI shows outcome

### Telemetry

1.  Device posts heartbeat/events
2.  Backend updates DeviceStatus and writes DeviceEvent
3.  Alerts process flags issues

------------------------------------------------------------------------

## 29.8 Scalability & Availability

-   Player endpoints scale horizontally.
-   Telemetry can be queued asynchronously at higher scale.
-   DB indexes on manifest resolution paths.
-   Separate scaling policies for user and player traffic recommended.

------------------------------------------------------------------------

## 29.9 Deployment Topology (Recommended)

Minimum production: - User API service - Player API service (or separate
autoscaling group) - Worker service - Managed relational DB - Managed
Redis - Storage bucket + CDN - Observability stack

------------------------------------------------------------------------

End of Backend Service Architecture section.

------------------------------------------------------------------------

# 30. Timezone Handling

This section defines how timezones are represented and applied across
tenants, venues, devices, scheduling, and manifest resolution. Correct
timezone handling is essential for predictable "time of day" scheduling,
especially around Daylight Saving Time (DST) transitions.

## 30.1 Principles

-   Scheduling is defined in local venue time.
-   All timestamps stored in the database are UTC.
-   Timezone selection follows Tenant → Venue → Device precedence.
-   Schedule resolution must handle DST transitions deterministically.
-   The manifest includes `validToUtc` so players switch correctly
    without needing complex timezone logic on-device.

------------------------------------------------------------------------

## 30.2 Timezone Sources and Precedence

### Tenant

-   `Tenant.defaultTimezone` (IANA string, e.g., `Europe/London`)
-   Used if venue timezone not specified.

### Venue

-   `Venue.timezone`
-   Primary timezone for schedule interpretation.

### Device (optional)

-   `Device.timezone`
-   Used when screens in a venue operate in different timezones or
    devices move temporarily.

### Precedence Order

1.  Device timezone
2.  Venue timezone
3.  Tenant default timezone

This result is the **effectiveTimezone**.

------------------------------------------------------------------------

## 30.3 Storage Formats

### UTC timestamps

Always stored as UTC: - audit timestamps - publish timestamps -
telemetry timestamps - pairing expirations - commands and events

### Schedule time-of-day values

Stored as local time-of-day values interpreted in effectiveTimezone.

Recommended: - `startMinutes` (0--1439) - `endMinutes` (0--1439)

Alternative: - `"HH:mm"` local strings.

Optional date bounds: - `validFromDate` - `validToDate`

------------------------------------------------------------------------

## 30.4 Schedule Interpretation Rules

### Weekday matching

Weekdays are evaluated in venue local time.

### Midnight wrap slots

If end ≤ start, slot wraps across midnight.

Example: 21:00--02:00 applies both before and after midnight as
continuation of prior day.

Resolver must evaluate both current and wrap-from-previous-day cases.

------------------------------------------------------------------------

## 30.5 DST Handling

### Spring forward

When local time skips forward: - Slot logic follows wall-clock intent. -
Resolver selects correct slot at first valid instant. - No infinite
loops or crashes allowed.

### Fall back

When local time repeats: - Slot evaluation remains wall-clock based. -
Resolver must avoid oscillation between playlists. - Validity windows
computed in UTC avoid thrashing.

------------------------------------------------------------------------

## 30.6 Manifest Validity

Resolver returns: - `validToUtc` - `nextCheckSeconds`

`validToUtc` usually equals next schedule boundary or a safety horizon
limit.

Players refresh shortly before expiry and apply updates at safe playback
boundaries.

------------------------------------------------------------------------

## 30.7 Resolver Timezone Requirements

Resolver must:

1.  Determine effective timezone.
2.  Convert UTC now → local time.
3.  Compute day-of-week and minutes since midnight.
4.  Evaluate active slots including wrap logic.
5.  Apply precedence rules.
6.  Select playlist.
7.  Compute next change instant.

------------------------------------------------------------------------

## 30.8 UI Expectations

Scheduling UI always shows venue-local time.

If user timezone differs: - Venue time remains authoritative.

Audit views may optionally show both UTC and local representations.

------------------------------------------------------------------------

## 30.9 Telemetry Handling

Device timestamps are accepted but server receive time is authoritative
if device clocks are incorrect.

All stored timestamps remain UTC.

------------------------------------------------------------------------

## 30.10 Validation Rules

On update: - Times must be valid bounds. - Date ranges valid. - Timezone
identifiers valid IANA values.

On publish: - Venue timezone must exist or fallback applies.

------------------------------------------------------------------------

End of Timezone Handling section.

------------------------------------------------------------------------

# 31. Schedule Resolver Algorithm

This section formally defines how the backend determines which playlist
a device should play at any given moment.

The resolver runs whenever: - A player requests a manifest. - A manifest
cache entry expires. - A venue epoch changes. - Publishing occurs.

------------------------------------------------------------------------

## 31.1 Resolver Inputs

Inputs required:

From device: - deviceId - venueId - groupId (optional) - device settings

From time: - currentUtcTime

From venue: - venue timezone

From DB snapshots: - Published schedule for venue - Published
playlists - Device/group associations

------------------------------------------------------------------------

## 31.2 Resolver Output

Resolver must produce:

-   winning playlistId
-   publishedVersion
-   playlist items
-   validity window (`validToUtc`)
-   nextCheckSeconds

------------------------------------------------------------------------

## 31.3 Slot Matching Process

Step 1 --- Compute effective timezone.

Step 2 --- Convert UTC now to local time:

    localNow = convertUtcToTimezone(nowUtc, tz)

Step 3 --- Compute:

    localMinutes = hour * 60 + minute
    weekday = localNow.weekday

Step 4 --- Collect candidate slots:

A slot matches if: - enabled = true - weekday matches OR wrap
continuation applies - localMinutes between start/end OR wrap rule -
within optional date range - target matches device or its group

------------------------------------------------------------------------

## 31.4 Slot Precedence Rules

Slots are ordered by:

1.  Target specificity
    -   DEVICE overrides GROUP
2.  Priority value
3.  Most recently updated
4.  Stable ID ordering

Winner is first slot after ordering.

------------------------------------------------------------------------

## 31.5 Fallback Rules

If no slot matches:

Resolver checks in order:

1.  Venue fallback playlist
2.  Tenant default playlist (optional)
3.  Last published playlist (optional)
4.  No playlist → error manifest

------------------------------------------------------------------------

## 31.6 Validity Window Computation

Resolver computes next moment result could change:

Possible boundaries: - slot end - slot start - wrap start/end - validity
date boundaries - higher-priority slot start

Choose nearest future boundary.

Then:

    validToUtc = boundaryUtc

Clamp:

    validToUtc <= now + MAX_VALIDITY (e.g., 6h)

------------------------------------------------------------------------

## 31.7 Performance Requirements

Resolver must: - Avoid scanning all slots per request. - Use indexed
queries by venue. - Cache published schedules. - Cache playlist
snapshots.

Resolver execution target: - \<5 ms average per request.

------------------------------------------------------------------------

## 31.8 Caching Strategy

Manifest cache key:

    (venueId, deviceGroupId, deviceId*, epoch)

*`deviceId` is included in the cache key only when the device has
device-specific schedule overrides or a device-level timezone. Devices
with no overrides share the group-level cached manifest.

Cache invalidated when:

-   Venue epoch changes.
-   Device group assignment changes.
-   Device reassigned to a different venue.
-   Device-specific schedule slot added or removed.

TTL typically 30--60 seconds.

------------------------------------------------------------------------

## 31.9 Failure Handling

If resolver fails: - Return cached manifest if available. - Otherwise
return minimal fallback manifest.

System must never cause all screens to go blank due to resolver failure.

------------------------------------------------------------------------

## 31.10 Determinism Requirement

Resolver must be deterministic:

Given same: - time - schedule - device - playlists

It must always produce identical results.

------------------------------------------------------------------------

End of Schedule Resolver section.

------------------------------------------------------------------------

# 32. Platform Technology Stack Guidance

This section defines the preferred technology stack for all platform
components:

1.  Fire TV Player Application
2.  Mobile Management Application
3.  Backend Services and Infrastructure

The goal is to standardize implementation choices, reduce operational
complexity, and ensure consistent developer tooling across the platform.

Unless explicitly overridden, implementations SHOULD follow this
guidance.

------------------------------------------------------------------------

## 32.1 Fire TV Player Application

The Fire TV application is a playback-critical runtime and must
prioritize performance, reliability, and device control.

### Platform

-   Amazon Fire OS (Android-based)

### Implementation Requirements

The player SHALL be implemented as a native Android application, not
using cross-platform frameworks.

### Recommended Technology Stack

  Area              Technology
  ----------------- -----------------------------------------
  Language          Kotlin (preferred) or Java
  Framework         Native Android SDK
  UI                Native Android Views or Jetpack Compose
  Media Playback    ExoPlayer
  Networking        OkHttp / Retrofit
  Local Storage     SQLite + filesystem cache
  Background Work   WorkManager / Foreground Services
  Packaging         Android APK

### Rationale

Native implementation provides:

-   Stable long-running playback loops
-   Reliable background downloads
-   Better media performance
-   Lower memory footprint
-   Improved kiosk/shared mode control
-   Predictable crash recovery
-   Better lifecycle handling

Cross-platform runtimes introduce overhead and lifecycle constraints
unsuitable for kiosk-style playback devices.

------------------------------------------------------------------------

## 32.2 Mobile Management Application

The mobile application is used for venue and device management, playlist
editing, and scheduling.

This application benefits from cross-platform development to support
both iOS and Android.

### Platform Targets

-   iOS
-   Android

### Recommended Technology Stack

  Area              Technology
  ----------------- ----------------------------------
  Framework         React Native
  Runtime/Tooling   Expo
  Language          TypeScript
  Networking        REST over HTTPS
  Authentication    JWT token flows
  Storage           Secure local storage
  UI Components     React Native component libraries

### Rationale

React Native with Expo allows:

-   Shared codebase across platforms
-   Faster iteration
-   Lower development overhead
-   Easier updates for admin tooling
-   Good performance for management workflows

Playback-critical constraints do not apply to this application.

------------------------------------------------------------------------

## 32.3 Backend Services

Backend services support scheduling, content management, device
communication, telemetry, and operations.

### Backend Application Platform

Backend services SHOULD be implemented using:

-   Microsoft .NET (latest LTS)
-   ASP.NET Core
-   C#

### API Style

Services SHALL expose:

-   REST APIs
-   JSON payloads
-   JWT authentication
-   Standard HTTP semantics

------------------------------------------------------------------------

### Database Platform

Primary relational database:

-   PostgreSQL

Used for: - tenant and venue data - schedules - playlists - devices -
telemetry metadata - audit history

------------------------------------------------------------------------

### Caching & Coordination

Recommended:

-   Redis

Used for: - manifest caching - presence indicators - rate limiting -
command delivery optimization - coordination data

------------------------------------------------------------------------

### Background Processing

Background processing SHOULD use:

-   .NET hosted services or worker services
-   Queue-backed processing where scaling requires

Used for: - asset processing - cleanup jobs - telemetry aggregation -
retry pipelines

------------------------------------------------------------------------

### Media Storage & Delivery

Media delivery SHALL use:

-   Object storage for assets
-   CDN distribution
-   Signed URLs for upload/download

Backend services must not proxy media downloads.

------------------------------------------------------------------------

### Hosting Model

Recommended:

-   Containerized services
-   Managed container/app hosting platforms
-   Horizontal scaling support
-   Rolling deployments

Cloud provider is not mandated.

------------------------------------------------------------------------

### Observability

Backend systems must support:

-   Centralized logging
-   Metrics collection
-   Health checks
-   Alerting integration

OpenTelemetry-compatible tracing is recommended.

------------------------------------------------------------------------

### Development & CI/CD

Recommended practices:

-   Git-based workflows
-   Automated builds and tests
-   Automated deployment pipelines
-   Environment promotion stages

CI/CD tooling is not mandated.

------------------------------------------------------------------------

## 32.4 Technology Evolution Policy

Technology choices may evolve, but:

-   API contracts must remain stable.
-   Migration paths must exist.
-   Device compatibility must not break.

Changes should be recorded in future specification revisions.

------------------------------------------------------------------------

End of Platform Technology Stack Guidance section.

------------------------------------------------------------------------

# 33. Player Internal Architecture

This section defines the internal architecture of the Fire TV Player
application. The goal is to ensure consistent implementation across
teams while maintaining high playback reliability, offline resilience,
and predictable device behavior.

The player is designed as a long-running kiosk-style application
responsible for media playback, asset caching, backend synchronization,
and device telemetry.

------------------------------------------------------------------------

## 33.1 Architectural Goals

The player architecture must:

-   Maintain continuous playback without blank screens.
-   Support offline playback indefinitely when content exists locally.
-   Recover automatically from crashes or restarts.
-   Download and manage media assets efficiently.
-   Apply schedule changes safely.
-   Remain stable on low-resource devices.
-   Operate reliably in kiosk and shared modes.

------------------------------------------------------------------------

## 33.2 High-Level Module Architecture

The player SHOULD be implemented using modular components with clear
responsibilities.

Logical module layout:

+---------------------------------------+
|              Player App               |
+---------------------------------------+
| App Controller / Lifecycle Manager    |
+---------------------------------------+
| Playback Engine Manifest & Sync       |
| Engine Asset Download Manager Cache   |
| Manager Command Executor Telemetry    |
| Client Device Configuration Manager   |
+---------------------------------------+
| Storage Layer (DB + Filesystem)       |
+---------------------------------------+
| Android System Services               |
+---------------------------------------+

Each module must operate independently where possible to reduce
coupling.

------------------------------------------------------------------------

## 33.3 Core Modules

### 33.3.1 App Controller

Coordinates application startup and lifecycle.

Responsibilities: - Initialize modules on boot. - Restore previous
playback state. - Manage foreground state. - Restart playback loop after
crash recovery. - Coordinate safe shutdown/restart.

------------------------------------------------------------------------

### 33.3.2 Playback Engine

Responsible for media playback.

Responsibilities: - Ordered playlist playback. - Display images for
configured duration. - Play video content fully. - Handle playback
failures gracefully. - Support safe transitions between items. - Avoid
mid-playlist interruptions.

Implementation guidance: - Use ExoPlayer for video playback. - Use
native rendering views for images.

------------------------------------------------------------------------

### 33.3.3 Manifest & Sync Engine

Handles communication with backend.

Responsibilities: - Poll manifest endpoint. - Detect manifest changes. -
Stage new manifests. - Trigger asset downloads. - Apply manifest updates
safely.

Must support: - Conditional GET via ETags. - Backoff during outages. -
Offline continuation.

------------------------------------------------------------------------

### 33.3.4 Asset Download Manager

Handles downloading required assets.

Responsibilities: - Queue downloads. - Limit concurrency. - Resume
interrupted downloads. - Verify checksums. - Perform atomic writes.

Downloads should not block playback.

------------------------------------------------------------------------

### 33.3.5 Cache Manager

Manages local storage space.

Responsibilities: - Track stored assets. - Protect assets in current
playlist. - Evict unused assets when space needed. - Prevent storage
exhaustion.

Eviction strategy: - Least recently used among non-protected assets.

------------------------------------------------------------------------

### 33.3.6 Command Executor

Executes backend-issued commands.

Responsibilities: - Poll commands endpoint. - Execute commands at safe
playback points. - Persist last processed command. - Report command
results.

Example commands: - Manifest refresh - Restart playback - Cache
cleanup - Log level change

------------------------------------------------------------------------

### 33.3.7 Telemetry Client

Reports device state and events.

Responsibilities: - Send heartbeat. - Batch playback and error events. -
Retry failed transmissions. - Limit local event queue size.

Telemetry must never block playback.

------------------------------------------------------------------------

### 33.3.8 Device Configuration Manager

Maintains runtime configuration.

Responsibilities: - Apply device settings. - Manage kiosk/shared mode. -
Update runtime flags. - Enforce keep-awake behavior.

------------------------------------------------------------------------

## 33.4 Storage Architecture

Player stores data locally:

Persistent storage: - SQLite database for metadata - Filesystem storage
for assets - Manifest files

Stored data types: - Cached assets - Manifest snapshots - Playback
progress - Command state - Telemetry queue

Atomic updates must be used to avoid corruption.

------------------------------------------------------------------------

## 33.5 Threading Model

Recommended threading model:

Main thread: - UI and playback coordination.

Background threads: - Manifest polling. - Downloads. - Cache eviction. -
Telemetry transmission.

Long operations must not run on UI thread.

------------------------------------------------------------------------

## 33.6 Crash Recovery

Player must recover automatically.

On restart: 1. Load cached manifest. 2. Validate playable assets. 3.
Resume playback loop. 4. Resume sync loops.

Crash loops should be detectable and mitigated.

------------------------------------------------------------------------

## 33.7 Offline Behavior

If backend unreachable: - Continue playback from cache. - Retry sync
using backoff. - Log telemetry locally.

Player must remain operational offline.

------------------------------------------------------------------------

## 33.8 Update Safety

Manifest updates must: - Never interrupt playing video. - Apply only at
safe boundaries. - Preserve fallback manifests.

------------------------------------------------------------------------

## 33.9 Resource Constraints

Player must operate on low-resource devices.

Constraints: - Limited memory - Limited CPU - Limited storage - Variable
network quality

Player should: - Limit concurrent downloads. - Avoid memory-heavy
operations. - Avoid excessive background tasks.

------------------------------------------------------------------------

## 33.10 Deterministic Playback

Playback order must be deterministic.

Given identical manifest and assets, playback sequence must always match
across devices.

------------------------------------------------------------------------

End of Player Internal Architecture section.

------------------------------------------------------------------------

# 34. Deployment & Scaling Architecture

This section defines recommended deployment architecture and scaling
strategies for production environments. The platform must support growth
from small venue installations to large multi-tenant deployments without
redesign.

------------------------------------------------------------------------

## 34.1 Deployment Goals

The deployment architecture must:

-   Provide high availability for playback-critical services.
-   Scale horizontally with device growth.
-   Support zero-downtime updates.
-   Isolate user-facing and device-facing workloads.
-   Support staged rollout and rollback.
-   Enable cost-efficient scaling.

------------------------------------------------------------------------

## 34.2 Environment Model

Typical environments:

-   Development
-   Test / QA
-   Staging
-   Production

Each environment should have isolated:

-   Databases
-   Storage buckets
-   API deployments
-   Monitoring configuration

Production environments should support regional expansion.

------------------------------------------------------------------------

## 34.3 Service Deployment Units

Services should be deployable independently:

1.  User API / BFF service
2.  Player Edge API service
3.  Background worker services
4.  Database services
5.  Cache services
6.  Storage + CDN

------------------------------------------------------------------------

## 34.4 Containerization

Services SHOULD run in containers.

Benefits:

-   Consistent runtime
-   Simplified scaling
-   Predictable deployments
-   Easier rollback

Images should be immutable and versioned.

------------------------------------------------------------------------

## 34.5 Horizontal Scaling Strategy

### Player Edge API scaling

Most load originates from devices polling manifests.

Approximate load:

Requests per second ≈ device_count / poll_interval_seconds

Examples:

-   1,000 devices → \~17 req/s
-   10,000 devices → \~167 req/s
-   50,000 devices → \~833 req/s

Scaling methods:

-   Horizontal replicas
-   Load balancer distribution
-   Manifest caching
-   Autoscaling based on CPU or request rate

------------------------------------------------------------------------

### User API scaling

Admin operations produce lighter load.

Scaling based on:

-   Active users
-   Publish operations
-   Upload frequency

Autoscaling typically lower priority than device endpoints.

------------------------------------------------------------------------

## 34.6 Database Scaling Strategy

Initial deployment:

-   Single primary DB instance with optional read replicas.

Growth strategy:

-   Vertical scaling first.
-   Add read replicas for analytics/admin queries.
-   Partition heavy telemetry tables if needed.

Avoid early sharding complexity.

------------------------------------------------------------------------

## 34.7 Cache Scaling

Redis deployment should support:

-   Replication
-   Persistence
-   Failover

Manifest caching dramatically reduces DB load.

------------------------------------------------------------------------

## 34.8 Storage & CDN Scaling

Media delivery must scale independently.

Strategy:

-   Assets stored in object storage.
-   Devices download directly from CDN.
-   Backend never proxies media.

Scaling primarily handled by CDN provider.

------------------------------------------------------------------------

## 34.9 Worker Scaling

Workers scale based on:

-   Upload processing volume
-   Telemetry aggregation load
-   Cleanup operations

Queue-based worker scaling recommended for large installations.

------------------------------------------------------------------------

## 34.10 Deployment Strategy

Recommended production deployment:

-   Rolling updates
-   Health checks before traffic switch
-   Automatic rollback on failure
-   Canary deployments for risky updates

Device APIs should avoid breaking changes.

------------------------------------------------------------------------

## 34.11 Disaster Recovery

Production environments must support:

-   Automated database backups
-   Storage redundancy
-   Infrastructure redeployment
-   Backup restoration procedures

Recovery objectives:

-   RPO: minutes
-   RTO: under 1 hour recommended

------------------------------------------------------------------------

## 34.12 Cost Scaling Considerations

Primary cost drivers:

-   CDN bandwidth
-   Storage volume
-   Compute instances
-   Database size

Optimization strategies:

-   Asset reuse
-   Efficient caching
-   Compression
-   Controlled telemetry retention

------------------------------------------------------------------------

End of Deployment & Scaling Architecture section.
