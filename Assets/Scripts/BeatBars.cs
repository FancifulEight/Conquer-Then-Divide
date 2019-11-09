using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBars : MonoBehaviour {
    public int numberOfBars = 24;
    public Transform prefab;
    public Transform[] bars = new Transform[0];
    private int index = 0;
    // Start is called before the first frame update
    void Start() {
        bars = new Transform[numberOfBars];

        float yStart = GameManager.gm.spawnY;
        float div = 0.72f;
        for (int i = numberOfBars - 1;i >= 0;i--) {
            //bars[i] = Instantiate(prefab, transform);
            bars[i] = Instantiate(prefab, new Vector2(0, yStart), Quaternion.identity);
            bars[i].parent = transform;

            yStart -= div;
        }
    }

    public void RestartBar() {
        if (!GameManager.gm.IsMusicPlaying()) return;
        bars[index].position = new Vector2(0, GameManager.gm.spawnY);
        index = (index + 1) % bars.Length;
    }

    public void UpdateBars(float amount) {
        foreach (var item in bars)
        {
            item.Translate(0, amount, 0);
        }
    }
}
