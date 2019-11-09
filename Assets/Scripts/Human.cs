using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour{

    public int speed = 4;
    public bool dead = false;

    public SpriteRenderer rend;

    [Header("Army")]
    public bool mine = false;
    public int armyNum = 0;
    public float targetRange = 0.2f;

    [Header("If not Mine")]
    public Transform target;

    private float currentSpeed = 0;
    private Animator anim;
    
    // Start is called before the first frame update
    void Start() {
        if (mine)
            GameManager.gm.army.Add(this);
        GameManager.gm.allEntities.Add(this.transform);
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (dead) return;
        
        Vector2 targetPos;
        if (mine) {
            targetPos = GameManager.gm.armyGoals[armyNum];
        } else {
            targetPos = target.position;
        }

        Vector2 direction = targetPos - (Vector2)transform.position;
        bool stop = direction.magnitude < targetRange;
        direction.Normalize();

        currentSpeed = Mathf.Lerp(currentSpeed, (stop) ? 0:speed, 0.1f);
        anim.SetFloat("Motion", currentSpeed / 2);

        transform.Translate(direction * currentSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
    }

    public void Recruit() {
        if (!mine) {
            GameManager.gm.army.Add(this);
            rend.color = GameManager.gm.armyColour;
            mine = true;
        }
    }

    public void Die() {
        anim.SetTrigger("Die");
        if (mine) GameManager.gm.army.Remove(this);
        rend.color = Color.gray;
        dead = true;
    }

    public void OnDestroy() {
        if (GameManager.gm != null) {
            GameManager.gm.allEntities.Remove(this.transform);
        }
    }
}
