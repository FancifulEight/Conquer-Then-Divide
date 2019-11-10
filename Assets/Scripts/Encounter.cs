using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour {
    public enum ActionType {
        LMB, RMB
    };

    public ActionType actionToPerform = ActionType.LMB;
    public List<Human> actors = new List<Human>();
    public Human actorPrefab;

    public bool started = false;

    void Start() {
        if (!started) {
            if (actors.Count == 0 && actorPrefab != null) {
                //Create new actors
                int actorsToCreate = Math.Max(1, GameManager.gm.tickCount / 16);
                float change = 0; 
                for (int i = 0;i < actorsToCreate;i++) {
                    actors.Add(Instantiate(actorPrefab, new Vector2(transform.position.x + change, transform.position.y), Quaternion.identity));
                    actors[i].transform.parent = transform;

                    change += 0.1f;
                }
                GameManager.gm.ReorganizeArmy(actors, UnityEngine.Random.Range(1, 5));
            } else if (!actorPrefab.enemy) {
                foreach(Human actor in actors) {
                    actor.armyNum = UnityEngine.Random.Range(1, 4);
                }
            }

            //Debug.Log(actors);
            foreach (var item in actors) {        
                //Debug.Log(item);
                item.target = this.transform;
            }
        }

        GameManager.gm.allEntities.Add(transform);

        started = true;
    }

    void Update() {
        if (transform.position.y < -12) {
            if (actors.Count > 0) {
                if (actorPrefab.enemy) {
                    //TODO: Kill some of your army
                } else {
                    //Recruit them
                    Recruit(false);
                }
            }
            Destroy(gameObject);
        }
    }

    public void Recruit(bool destroyOnSuccess = true) {
        foreach (var item in actors)
        {
            item.Recruit();
            item.transform.parent = null;
        }

        GameManager.gm.UpdateScore(actors.Count * GameManager.gm.pointsPerRecruit);
        GameManager.gm.PlayPoofSFX();
        actors.Clear();

        GameManager.gm.armyChanged = true;

        if (destroyOnSuccess)
            Destroy(gameObject);
    }

    public void Kill() {
        foreach (var item in actors)
            item.Die();
            
        GameManager.gm.UpdateScore(actors.Count * GameManager.gm.pointsPerKill);
        //Clear so the actors can't be used again (They will be deleted along with the encounter, their parent object)
        actors.Clear();
    }

    public void OnDestroy() {
        if (GameManager.gm != null)
            GameManager.gm.allEntities.Remove(transform);
    }
}
