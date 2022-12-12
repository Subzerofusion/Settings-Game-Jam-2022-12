module NewGameProject.RayCast
open System
open Godot
open Godot.Collections
type RayResponse=
    {
        collider:Object // The colliding object.
        // collider_id:uint64 // The colliding object's ID.
        normal:Vector3 // The object's surface normal at the intersection point.
        position: Vector3 // The intersection point.
        rid:RID // The intersecting object's RID 
        // shape: uint32 // The shape index of the colliding shape.
    }


let parseRayResponse(response:Dictionary)=
    if response.Count=0 then None
    else
        Some{
            collider=response.Item("collider"):?>Object
            // collider_id=response.Item("collider_id"):?>uint64 // The colliding object's ID.
            normal=response.Item("normal"):?>Vector3 // The object's surface normal at the intersection point.
            position=response.Item("position"):?> Vector3 // The intersection point.
            rid=response.Item("rid"):?>RID // The intersecting object's RID 
            // shape=response.Item("shape"):?> uint32 // The shape index of the colliding shape.
        }

let rayCastFromMouse(camera3d:Camera) (length:float32) layerMask=
    let spaceState=camera3d.GetWorld().DirectSpaceState
    let mousePos=camera3d.GetViewport().GetMousePosition();
    let rayOrigin = camera3d.ProjectRayOrigin(mousePos)
    let rayEnd=rayOrigin + camera3d.ProjectRayNormal(mousePos) * length
    spaceState.IntersectRay(rayOrigin,rayEnd,collisionMask=layerMask)|>parseRayResponse
