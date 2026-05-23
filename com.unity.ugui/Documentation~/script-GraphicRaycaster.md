# Graphic Raycaster

The Graphic Raycaster is used to raycast against a Canvas. The Raycaster looks at all Graphics on the canvas and determines if any of them have been hit.

You can configure the Graphic Raycaster to ignore backfacing Graphics and to be blocked by 2D or 3D objects in front of it.

## Properties

|**_Property:_** |**_Function:_** |
|:---|:---|
|__Ignore Reversed Graphics__ | Should graphics facing away from the raycaster be considered? |
|__Blocking Objects__ | Type of objects that are checked to determine if they block graphic raycasts. |
|__Blocking Mask__ | Type of objects specified through LayerMask that are checked to determine if they block graphic raycasts. |
