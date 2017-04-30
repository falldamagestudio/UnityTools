# Disable assembly reload in Play mode

Does your project code not survive hot reloads? Put this script somewhere in your project structure.

Unity will no longer reload assemblies when in Play mode. It will still detect and reload non-code assets in Play mode.

Hot reload remains active in edit mode.

## References

[Unity blog post with a similar implementation](https://support.unity3d.com/hc/en-us/articles/210452343-How-to-stop-automatic-assembly-compilation-from-script)

[Forum thread](https://forum.unity3d.com/threads/solved-turning-off-the-auto-compile.422302/)
