using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour {
    public enum ActionType {
        LMB, RMB
    };

    public ActionType actionToPerform = ActionType.LMB;
    public List<Human> actors = new List<Human>();

    void Start() {
        foreach (var item in actors)
        {
            item.target = this.transform;
        }
    }

    void Update() {
        if (transform.position.y < -12)
            Destroy(gameObject);
    }

    public bool isCorrectAction(bool isLeft) {
        return (actionToPerform == ActionType.LMB && isLeft) || (actionToPerform == ActionType.RMB && !isLeft);
    }

    public bool InputGetMouse() {
        int mouse = (actionToPerform == ActionType.LMB) ? 0:1;
        return Input.GetMouseButtonDown(mouse) || Input.GetMouseButton(mouse);
    }

    public void Recruit() {
        foreach (var item in actors)
        {
            item.Recruit();
            item.transform.parent = null;
        }

        Destroy(gameObject);
    }

    public void Kill() {
        foreach (var item in actors)
        {
            item.Die();
        }
    }
}
