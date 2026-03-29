using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TennisScoreManager : MonoBehaviour
{
    [System.Serializable]
    public class GameState
    {
        public int aPoint, bPoint;
        public bool advA, advB;
        public int aGames, bGames;
        public int aSets, bSets;
        public List<string> setHistory;

        public GameState(int aPoint, int bPoint, bool advA, bool advB, int aGames, int bGames, int aSets, int bSets, List<string> setHistory)
        {
            this.aPoint = aPoint;
            this.bPoint = bPoint;
            this.advA = advA;
            this.advB = advB;
            this.aGames = aGames;
            this.bGames = bGames;
            this.aSets = aSets;
            this.bSets = bSets;
            this.setHistory = new List<string>(setHistory);
        }
    }


    // UI
    public TextMeshProUGUI teamAScoreText;
    public TextMeshProUGUI teamBScoreText;
    public TextMeshProUGUI teamAGamesText;
    public TextMeshProUGUI teamBGamesText;
    public TextMeshProUGUI teamASetsText;
    public TextMeshProUGUI teamBSetsText;
    public GameObject mainPanel;
    public GameObject scoreboardPanel;
    public TextMeshProUGUI scoreboardText;

    // Points
    int[] points = { 0, 15, 30, 40 };
    int aPoint = 0;
    int bPoint = 0;
    bool advA = false;
    bool advB = false;

    // Games
    int aGames = 0;
    int bGames = 0;

    // Sets
    int aSets = 0;
    int bSets = 0;

    // Set history
    List<string> setHistory = new List<string>();

    // Undo history
    Stack<GameState> undoStack = new Stack<GameState>();

    // Volume button press tracking
    private float multiPressTime = 0.5f; // max time between multiple presses
    private int volumePressCount = 0;
    private float lastPressTime = 0f;
    private bool waitingForPresses = false;

void Update()
{
#if UNITY_EDITOR
    // Press SPACE to simulate volume button
    if (Input.GetKeyDown(KeyCode.Space))
    {
        OnVolumeButtonPressed();
    }
#endif
}

   public void OnVolumeButtonPressed()
{
    float currentTime = Time.time;

    if (currentTime - lastPressTime > multiPressTime)
    {
        volumePressCount = 0;
    }

    volumePressCount++;
    lastPressTime = currentTime;

    if (!waitingForPresses)
    {
        waitingForPresses = true;
        StartCoroutine(ProcessVolumePresses());
    }
}
    IEnumerator ProcessVolumePresses()
    {
        yield return new WaitForSeconds(multiPressTime);

        // Execute action based on number of presses
        if (volumePressCount == 1)
        {
            AddPoint(true); // Team A
        }
        else if (volumePressCount == 2)
        {
            AddPoint(false); // Team B
        }
        else if (volumePressCount >= 3)
        {
            UndoLastPoint();
        }

        // Reset for next detection
        volumePressCount = 0;
        waitingForPresses = false;
    }

    void AddPoint(bool isA)
    {
        SaveState();

        // Deuce logic
        if (aPoint == 3 && bPoint == 3)
        {
            if (isA)
            {
                if (advA) WinGame(true);
                else if (advB) advB = false;
                else advA = true;
            }
            else
            {
                if (advB) WinGame(false);
                else if (advA) advA = false;
                else advB = true;
            }
        }
        else
        {
            if (isA)
            {
                if (aPoint < 3) aPoint++;
                else WinGame(true);
            }
            else
            {
                if (bPoint < 3) bPoint++;
                else WinGame(false);
            }
        }

        UpdateUI();
    }

    void WinGame(bool isA)
    {
        if (isA) aGames++; else bGames++;

        aPoint = 0;
        bPoint = 0;
        advA = advB = false;

        if ((aGames >= 6 || bGames >= 6) && Mathf.Abs(aGames - bGames) >= 2)
        {
            string setScore = aGames + "-" + bGames;
            setHistory.Add(setScore);

            if (isA) aSets++; else bSets++;
            Debug.Log("Set finished: " + setScore);

            aGames = 0;
            bGames = 0;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        teamAScoreText.text = GetPointText(aPoint, advA);
        teamBScoreText.text = GetPointText(bPoint, advB);
        teamAGamesText.text = aGames.ToString();
        teamBGamesText.text = bGames.ToString();
        teamASetsText.text = aSets.ToString();
        teamBSetsText.text = bSets.ToString();
    }

    string GetPointText(int point, bool adv)
    {
        return adv ? "AD" : points[point].ToString();
    }

    void SaveState()
    {
        undoStack.Push(new GameState(aPoint, bPoint, advA, advB, aGames, bGames, aSets, bSets, setHistory));
    }

    void UndoLastPoint()
    {
        if (undoStack.Count > 0)
        {
            GameState state = undoStack.Pop();
            aPoint = state.aPoint;
            bPoint = state.bPoint;
            advA = state.advA;
            advB = state.advB;
            aGames = state.aGames;
            bGames = state.bGames;
            aSets = state.aSets;
            bSets = state.bSets;
            setHistory = new List<string>(state.setHistory);

            UpdateUI();
            Debug.Log("Last point undone!");
        }
    }

    public void OpenScoreboard()
    {
        mainPanel.SetActive(false);
        scoreboardPanel.SetActive(true);
        UpdateScoreboardTable();
    }

    public void CloseScoreboard()
    {
        scoreboardPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void UpdateScoreboardTable()
    {
        string table = "SET | A | B\n";
        table += "----------------\n";

        int count = setHistory.Count;
        int start = Mathf.Max(0, count - 5);

        for (int i = start; i < count; i++)
        {
            string[] scores = setHistory[i].Split('-');

            table += (i + 1) + "   | "
                + scores[0] + " | "
                + scores[1] + "\n";
        }

        if (count == 0)
        {
            table += "No sets yet";
        }

        scoreboardText.text = table;
    }


}