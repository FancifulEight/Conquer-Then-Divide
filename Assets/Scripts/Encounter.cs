using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour {
    public enum ActionType {
        LMB, RMB
    };

    public ActionType actionToPerform = ActionType.LMB;
    public List<Human> actors = new List<Human>();

    public bool started = false;

    void Start() {
        foreach (var item in actors)
        {
            item.target = this.transform;
        }

        GameManager.gm.allEntities.Add(transform);

        started = true;
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

    public void PerformAction(bool success) {
        if (actionToPerform == ActionType.LMB) {
            if (success) {
                Recruit();
            } else {
                Kill();
            }
        } else if (actionToPerform == ActionType.RMB) {
            if (success) {
                Kill();
            } else {
                //TODO: Implement loss of army
            }
        }
    }

    public void Recruit() {
        foreach (var item in actors)
        {
            item.Recruit();
            item.transform.parent = null;
        }
        actors.Clear();

        Destroy(gameObject);
    }

    public void Kill() {
        foreach (var item in actors)
        {
            item.Die();
        }
        //Clear so the actors can't be used again (They will be deleted along with the encounter, their parent object)
        actors.Clear();
    }

    public void OnDestroy() {
        if (GameManager.gm != null)
            GameManager.gm.allEntities.Remove(transform);
    }
}
