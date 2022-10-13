using UnityEngine;

public class CollisionDetector : MonoBehaviour {
    public StickmanController Parent;

    private void Start() {
        Parent = FindObjectOfType<StickmanController>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Floor") {
            Parent.Death();
        }
    }
}
