using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCheck : MonoBehaviour {
    private StickmanController sc;
    public string id = "ll";
    void Start(){
        sc = FindObjectOfType<StickmanController>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Floor") {
            sc.GroundCheck(id, 1f);
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Floor") {
            sc.GroundCheck(id, -1f);
        }
    }
}
