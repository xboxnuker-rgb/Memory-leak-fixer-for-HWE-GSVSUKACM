# Changelog

## 1.0.0

- Track runtime material instances created by `WeedVisualsSetter.ApplyVisuals`.
- Release tracked materials when their owning `StoredItem` is destroyed.
- Sweep materials whose owner was destroyed through an alternate Unity path.
- Report aggregate capture/release counts and process private memory once per minute while active.

