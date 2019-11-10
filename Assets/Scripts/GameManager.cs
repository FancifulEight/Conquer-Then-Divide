using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    public static GameManager gm = null;
    [Header("Audio")]
    public AudioSource mainSrc;
    public AudioClip beat, song;
    public AudioSource sfxSrc;
    public AudioClip deathSFX, poofSFX;

    public float songLengthInSeconds = 48;
    public float beatTempo = 120;
    [Range(1, 8)]
    public int ticksPerBeat = 2;
    public int totalBeats;
    public bool magicNumberTotalBeats = false;

    private double startDSP, tickLength, nextTickTime;
    public int tickCount = 0;
    public UnityEvent ticked = new UnityEvent();

    public float beatBarSpeed = 2;

    public BeatBars bars;
    private bool startingUp = false, musicPlaying = false;

    [Header("UI")]
    public Text readyText;
    public GameObject[] armyPaths = new GameObject[4];
    public King king;

    public SpriteRenderer strikeZone;
    public Text scoreText;
    public int score = 0;

    public int pointsPerKill = 110;
    public int pointsPerRecruit = 100;

    [Header("Gameplay")]
    public Color armyColour = Color.blue;
    public Color enemyColour = Color.red;
    public Color neutralColour = Color.white;

    private float startTime = 0;
    public Vector2[] armyGoals = new Vector2[4];
    public int armySplit = 1;

    public List<Human> army = new List<Human>();
    public List<Transform> allEntities = new List<Transform>();

    public int spawnY = 8;
    public Encounter firstEncounter;
    public Encounter[] encountersPerBeat = new Encounter[0];
    private Encounter[] currentRun;

    private double lastLMB = 0, lastRMB = 0;
    public bool armyChanged = false;

    void OnValidate() {
        songLengthInSeconds = (song == null) ? 0: song.length;
        totalBeats = (magicNumberTotalBeats) ? totalBeats:(int)(beatTempo * songLengthInSeconds / 60);
        System.Array.Resize(ref encountersPerBeat, totalBeats);
    }

    void Awake() {
        if (gm == null) {
            gm = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start() {
        beatTempo /= 60;

        tickLength = 1f / (beatTempo * ticksPerBeat);
        nextTickTime = AudioSettings.dspTime + tickLength;

        ticked.AddListener(Tick);
        ticked.AddListener(BounceKing);
        ticked.AddListener(bars.RestartBar);
        
        mainSrc = GetComponent<AudioSource>();
        mainSrc.clip = beat;
        mainSrc.Play(); 
    }

    public void Tick() {
        if (!musicPlaying) return;

        int currentBeat = tickCount;// / ticksPerBeat;
        //Check if on beat, correct button is pressed
        if (currentRun.Length <= currentBeat) {
            Debug.Log(string.Format("Current encountersPerBeat Out of Range Exception Count = {0}", currentBeat));
            return;
        }
        //Debug.Log(currentBeat);
        if (currentRun[currentBeat] != null && currentRun[currentBeat].started) {
            //Check for hit
            if (ClickedOnBeat(true)) {
                //We Got A Hit!
                ButtonHit();
                //currentRun[currentBeat].PerformAction(true);
            } else {
                //We got a Miss...
                ButtonMissed();
                //currentRun[currentBeat].PerformAction(false);
            }
        }

        //Check to spawn encounters early
        int indexToCheck = (int)(currentBeat + spawnY / (beatBarSpeed * tickLength));
        if (currentRun.Length <= indexToCheck) {
            //We are near the end of the song
            return;
        }
        if (currentRun[indexToCheck] != null) {
            //Spawn it 
            currentRun[indexToCheck] = Instantiate(encountersPerBeat[indexToCheck], new Vector2(0, spawnY), Quaternion.identity);
        }
    }

    private int kingBobCount = 0;
    public int beatsPerBob = 8;
    public void BounceKing() {
        //Debug.Log("Count: " + kingBobCount);

        if (kingBobCount < beatsPerBob / 2)
            king.kingAnim.SetBool("Bob", true);
        else
            king.kingAnim.SetBool("Bob", false);
        
        kingBobCount = (kingBobCount + 1) % beatsPerBob;
    }

    public void Conquer() {
        armyChanged = true;
    }

    public void Divide() {
        armySplit = (armySplit + 1) % (armyGoals.Length + 1);
        if (armySplit == 0) armySplit++;

        for (int i = 0;i < armyPaths.Length;i++) {
            armyPaths[i].SetActive(i <= armySplit - 1);
        }
        
        armyChanged = true;
    }

    public void ReorganizeArmy(List<Human> list, int divisions) {
        for(int i = 0;i < divisions;i++)
            for (int j = i * list.Count / divisions;j < (i + 1) * list.Count / divisions && j < list.Count;j++)
                list[j].armyNum = i;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        //King Semaphore
        king.ChangeFlags(Input.GetMouseButton(0), Input.GetMouseButton(1));

        //Ticks
        startDSP = AudioSettings.dspTime;
        startDSP += Time.deltaTime;

        while(startDSP > nextTickTime) {
            ticked.Invoke();
            nextTickTime += tickLength;
            tickCount++;
        }
        //End Ticks

        bool left = Input.GetMouseButtonDown(0);
        bool right = Input.GetMouseButtonDown(1);
        if (left)
            lastLMB = AudioSettings.dspTime;
        if (right)
            lastRMB = AudioSettings.dspTime;

        strikeZone.color = new Color(
            strikeZone.color.r, strikeZone.color.g, strikeZone.color.b, 
            Mathf.Lerp(strikeZone.color.a, 
                (ClickedOnBeat(true) || ClickedOnBeat(false)) ? 0.9f:0.1f, 0.6f));

        if (left && !musicPlaying && !startingUp) {
            if (firstEncounter != null)
                firstEncounter.Recruit();
            mainSrc.loop = false;
            startingUp = true;

            readyText.text = "READY";
        }

        if (!mainSrc.isPlaying) {
            if (startingUp) {
                startingUp = false;
                readyText.text = "";
                
                tickCount = 0;
                currentRun = (Encounter[])encountersPerBeat.Clone();

                StartMusic();
            } else {
                mainSrc.loop = false;
                mainSrc.clip = beat;
                musicPlaying = false;

                readyText.text = "LMB TO\nCONTINUE";
            }
            mainSrc.Play();

        } else if (musicPlaying) {
            UpdateInGame();
        }

        if (armyChanged) {        
            ReorganizeArmy(army, armySplit);
            armyChanged = false;
        }
    }

    public void UpdateInGame() {
        if (Input.GetMouseButtonDown(0))
            Conquer();
        if (Input.GetMouseButtonDown(1))
            Divide();

        //My Army
        foreach (Transform t in allEntities) {
            t.Translate(0, -beatBarSpeed * Time.deltaTime, 0);
        }
        bars.UpdateBars(-beatBarSpeed * Time.deltaTime);
    }

    public void StartMusic() {
        musicPlaying = true;
        mainSrc.loop = false;
        mainSrc.clip = song;

        startTime = Time.time;
    }

    public bool IsMusicPlaying() {
        return musicPlaying;
    }

    public void ButtonHit() {
        Debug.Log("Hit On Time");
    }

    public void ButtonMissed() {
        Debug.Log("Missed Note");
    }

    public bool ClickedOnBeat(bool left) {
        return AudioSettings.dspTime - tickLength <= ((left) ? lastLMB:lastRMB);
    }

    public void UpdateScore(int pointsToAdd) {
        score += pointsToAdd;

        score = Math.Min(999999, score);

        int temp = score;
        int count = 6;
        while (temp > 0) {
            temp /= 10;
            count--;
        }

        string output = "Score: ";
        for (int i = 0;i < count;i++) {
            output += "0";
        }

        output += score;
        scoreText.text = output;
    }

    public void PlayDeathSFX() {
        sfxSrc.PlayOneShot(deathSFX, 0.2f + UnityEngine.Random.Range(0f, 0.4f));
    }

    public void PlayPoofSFX() {
        sfxSrc.PlayOneShot(poofSFX, 0.6f + UnityEngine.Random.Range(0f, 0.4f));
    }
}
