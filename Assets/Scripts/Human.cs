using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour{
    public int speed = 4;
    public bool dead = false;

    public SpriteRenderer rend;
    public ParticleSystem poofParticles;

    [Header("Army")]
    public bool mine = false;
    public bool enemy = false;
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
            targetPos = new Vector2(GameManager.gm.armyGoals[armyNum].x, target.position.y);
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

            poofParticles.Play();
        }
    }

    public void Die() {
        GameManager.gm.PlayDeathSFX();
        anim.SetTrigger("Die");
        if (mine) GameManager.gm.army.Remove(this);
        rend.color = Color.gray;
        dead = true;
    }

    public void OnDestroy() {
        if (GameManager.gm != null)
            GameManager.gm.allEntities.Remove(this.transform);
    }

    private int lastTickCollisionTime = -1;
    public void OnCollisionEnter2D(Collision2D collision) {
        if (!mine || dead) return;
        Human intruder = collision.gameObject.GetComponent<Human>();
        if (intruder == null || intruder.mine || intruder.dead) return;

        if (lastTickCollisionTime != GameManager.gm.tickCount) {
            if (!intruder.enemy) {//You collided with a civilian, the civilian will die
                intruder.Die();
            } else if (GameManager.gm.ClickedOnBeat(true)) { // You left clicked, which only makes it so you can kill enemies
                if (intruder.enemy) {
                    intruder.Die();
                    GameManager.gm.UpdateScore(GameManager.gm.pointsPerKill);
                }
            } else { // You didn't left click and you collided with an enemy. Upon next collision you will die.
                lastTickCollisionTime = GameManager.gm.tickCount;
            }
        } else {
            Die();
            GameManager.gm.armyChanged = true;
        }
    }
}
