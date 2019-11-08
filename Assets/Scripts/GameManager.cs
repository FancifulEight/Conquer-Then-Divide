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

    public float songLengthInSeconds = 48;
    public float beatTempo = 120;
    [Range(1, 8)]
    public int ticksPerBeat = 2;
    public int totalBeats;
    public bool magicNumberTotalBeats = false;

    private double startDSP, tickLength, nextTickTime;
    private int tickCount = 0;
    public UnityEvent ticked = new UnityEvent();

    [Header("UI")]
    public Text readyText;
    public GameObject[] armyPaths = new GameObject[4];
    public King king;

    [Header("Gameplay")]
    public Color armyColour = Color.blue;
    private float startTime = 0;
    public Vector2[] armyGoals = new Vector2[4];
    public int armySplit = 1;

    public List<Human> army = new List<Human>();
    public List<Transform> allEntities = new List<Transform>();

    public Encounter firstEncounter;
    public Encounter[] encountersPerBeat = new Encounter[0];

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
        
        mainSrc = GetComponent<AudioSource>();
        mainSrc.clip = beat;
        mainSrc.Play(); 
    }

    public void Tick() {
        if (!musicPlaying) return;

        int currentBeat = tickCount;// / ticksPerBeat;
        //Check if on beat, correct button is pressed
        if (encountersPerBeat.Length <= currentBeat) {
            Debug.Log(string.Format("Current encountersPerBeat Out of Range Exception Count = {0}", currentBeat));
            return;
        }
        if (encountersPerBeat[currentBeat] != null) {
            //Check for hit
            if (encountersPerBeat[currentBeat].InputGetMouse()) {
                //We Got A Hit!
                ButtonHit();
                encountersPerBeat[currentBeat].PerformAction(true);
            } else {
                //We got a Miss...
                ButtonMissed();
                encountersPerBeat[currentBeat].PerformAction(false);
            }
        }

        //Check to spawn encounters early
        int indexToCheck = currentBeat + 2;
        if (encountersPerBeat.Length <= indexToCheck) {
            //We are near the end of the song
            return;
        }
        if (encountersPerBeat[indexToCheck] != null) {
            //Spawn it 
            encountersPerBeat[indexToCheck] = Instantiate(encountersPerBeat[indexToCheck], new Vector2(3 * Random.Range(-2, 2), 10), Quaternion.identity);
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
        if (armySplit == 1) return;
        armySplit = 1;

        for (int i = 0;i < armyPaths.Length;i++) {
            armyPaths[i].SetActive(i == armySplit - 1);
        }

        ReorganizeArmy();
    }

    public void Divide() {
        if (armySplit == armyGoals.Length) return;
        armySplit++;

        armyPaths[armySplit - 1].SetActive(true);
        
        ReorganizeArmy();
    }

    public void ReorganizeArmy() {
        for(int i = 0;i < armySplit;i++)
            for (int j = i * army.Count / armySplit;j < (i + 1) * army.Count / armySplit && j < army.Count;j++)
                army[j].armyNum = i;
    }

    private bool startingUp = false, musicPlaying = false;
    // Update is called once per frame
    void Update() {
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
        if ((left || right) && !musicPlaying && !startingUp) {
            if (firstEncounter != null)
                if (firstEncounter.isCorrectAction(left)) {
                    firstEncounter.Recruit();
                } else {
                    firstEncounter.Kill();
                }
            mainSrc.loop = false;
            startingUp = true;
            tickCount = 0;

            readyText.text = "READY";

        }

        if (!mainSrc.isPlaying) {
            if (startingUp) {
                startingUp = false;
                readyText.text = "";

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
    }

    public void UpdateInGame() {
        if (Input.GetMouseButtonDown(0))
            Conquer();
        if (Input.GetMouseButtonDown(1))
            Divide();

        //My Army
        foreach (Transform t in allEntities) {
            t.Translate(0, -2 * Time.deltaTime, 0);
        }
    }

    public void StartMusic() {
        musicPlaying = true;
        mainSrc.loop = false;
        mainSrc.clip = song;

        startTime = Time.time;
    }

    public void ButtonHit() {
        Debug.Log("Hit On Time");
    }

    public void ButtonMissed() {
        Debug.Log("Missed Note");
    }
}
