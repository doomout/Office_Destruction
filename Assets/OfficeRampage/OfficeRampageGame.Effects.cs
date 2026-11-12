using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool mushroomCamera;
        private Vector3 mushroomCenter;
        private OfficeBlastEffect CreateBlast(Vector3 center, bool nuclear)
        {
            var obj = new GameObject(nuclear ? "BFG mushroom cloud" : "Rocket fire and smoke");
            obj.transform.SetParent(world); obj.transform.position = center;
            var effect = obj.AddComponent<OfficeBlastEffect>(); effect.Initialize(this, nuclear); return effect;
        }
        private void CreateSlidingDoor(Room room, Transform root)
        {
            room.gate = new GameObject("Sliding exit door"); room.gate.transform.SetParent(root);
            room.gate.transform.position = new Vector3(0, 0, room.end);
            var barrier = room.gate.AddComponent<BoxCollider>(); barrier.center = new Vector3(0, 1.55f, 0); barrier.size = new Vector3(3.3f, 3.1f, .35f);
            var door = room.gate.AddComponent<OfficeSlidingDoor>(); room.door = door; door.game = this; door.barrier = barrier;
            door.left = Shape(room.gate.transform, "Left sliding panel", new Vector3(-.825f, 1.55f, 0), new Vector3(1.64f, 3.1f, .28f), new Color(.16f, .24f, .3f), false).transform;
            door.right = Shape(room.gate.transform, "Right sliding panel", new Vector3(.825f, 1.55f, 0), new Vector3(1.64f, 3.1f, .28f), new Color(.16f, .24f, .3f), false).transform;
            // Details are parented to panels so they move with the door. Compensate for panel scaling.
            foreach (var panel in new[] { door.left, door.right })
                Shape(panel, "Door warning stripe", new Vector3(0, 0, -.54f), new Vector3(.75f, .045f, .04f), Orange, false);
            var a = Shape(room.gate.transform, "Left jamb", new Vector3(-1.82f, 1.65f, 0), new Vector3(.22f, 3.3f, .5f), Navy, false);
            var b = Shape(room.gate.transform, "Right jamb", new Vector3(1.82f, 1.65f, 0), new Vector3(.22f, 3.3f, .5f), Navy, false);
            var top = Shape(room.gate.transform, "Door status light", new Vector3(0, 3.32f, -.05f), new Vector3(3.9f, .2f, .5f), Orange, false);
            door.indicators = new[] { top.GetComponent<Renderer>(), a.GetComponent<Renderer>(), b.GetComponent<Renderer>() };
            door.openMaterial = Mat(Cyan);
            var sign = new GameObject("Door status").AddComponent<TextMesh>(); sign.transform.SetParent(room.gate.transform, false);
            sign.transform.localPosition = new Vector3(0, 2.6f, -.25f); sign.anchor = TextAnchor.MiddleCenter;
            sign.characterSize = .12f; sign.fontSize = 44; sign.color = Color.white; sign.text = "LOCKED"; door.sign = sign;
            room.gateRenderer = top.GetComponent<Renderer>();
        }
    }
}
