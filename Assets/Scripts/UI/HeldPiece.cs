using UnityEngine;

public class HeldPiece : MonoBehaviour
{
    void Update()
    {
        transform.position = FlattenVector(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    Vector3 FlattenVector(Vector3 pos)
    {
        return new Vector3(pos.x, pos.y, 0);
    }
}
