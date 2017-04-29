# Disable assembly reload in Play mode

Does your project code not survive hot reloads? Put this script somewhere in your project structure.

Unity will no longer reload assemblies when in Play mode. It will still detect and reload non-code assets in Play mode.

Hot reload remains active in edit mode.