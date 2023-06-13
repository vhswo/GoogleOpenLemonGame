using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Random = System.Random;

/// <summary>
/// 로그인 좀 더 손보기
/// 로그인 실패하면 로그인 버튼 안 사라지게
/// 로그인 안해도 겜은 가능 한데 랭킹 못보는거 말해주기
/// </summary>
public class StartGame : MonoBehaviour
{
    public GameManager gameMgr;

    public Text score;
    public Text carryScore;
    public Text timerText;

    public int createLemon;
    public GameObject lemonPrefab;
    public GameObject lemonBoxUI;
    public GameObject carryOnLemonUI;

    public GameObject gameOver;
    public Text bestScore;

    LemonBox lemonBox = new();
    CarryOnLemon CarryOnLemon = new();

    string lemonObjname = "Lemon";
    string carryOnLemon;
    Vector2 exLocation;
    int number;
    public int nowScore;
    public float timer;

    GameObject clickedObj;
    public Action<StartGame> startObjScript;

    public void Awake()
    {
        Random rand = new();
        for(int i = 0; i < createLemon; i++)
        {
            GameObject lemon = Instantiate(lemonPrefab, lemonBoxUI.transform);

            lemon.gameObject.name = lemonObjname;
            lemonBox.lemons.Add(lemon,new(lemon));
            lemonBox.lemons[lemon].scoreText = lemon.transform.GetChild(0).GetComponent<Text>();
        }

        lemonBox.rect = lemonBoxUI.GetComponent<RectTransform>();

        CarryOnLemon.UI = carryOnLemonUI;
        carryOnLemon = carryOnLemonUI.name;
        CarryOnLemon.scoreText = carryScore;

    }

    private void OnEnable()
    {
        GameState = true;
        SettingStart();
    }

    public void SettingStart()
    {
        gameObject.SetActive(true);

        nowScore = 0;
        score.text = nowScore.ToString();

        timer = 99f;
        timerText.text = timer.ToString();

        lemonBox.SortLemon();
        CarryOnLemon.SetScore(new Random().Next(15, 25));

        startObjScript?.Invoke(this);
    }

    bool GameState = false;
    public void Update()
    {
        if (!GameState) return;
        if (timer <= 0)
        {
            GameOver();
            GameState = false;
        }

        LeftClick();

        timer -= Time.deltaTime;
        timerText.text = timer.ToString("F2");
    }

    public void CheckBox()
    {
        int find = CarryOnLemon.showScore; //찾아야할 점수
        
        int index = 0;
        int lastSocre =0;
        foreach(GameObject lemon in lemonBox.lemons.Keys)
        {
            if(lemon.activeSelf)
            {
                find -= lemonBox.lemons[lemon].score;

                if (find >= 0) break;
                
                find += lemonBox.lemons[lemon].score;
                lastSocre = lemonBox.lemons[lemon].score;
            }

            index++;

            if (index == lemonBox.lemons.Count && find - lastSocre <= 0) lemonBox.SortLemon();
        }

    }


    bool Clicked = false;
    public void LeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickedObj = ResultSlot();
            if (clickedObj != null && clickedObj.name == lemonObjname)
            {
                number = clickedObj.transform.GetSiblingIndex();
                exLocation = clickedObj.transform.localPosition;

                clickedObj.transform.SetAsLastSibling();

                Clicked = true;
            }
        }

        if (!Clicked) return;

        if (Input.GetMouseButton(0))
        {
            clickedObj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        }

        if (Input.GetMouseButtonUp(0))
        {
            GameObject endObj = ResultSlot(clickedObj);
            bool goToCarryBox = false;
            if(endObj != null)
            {
                if (endObj.name == carryOnLemon) goToCarryBox = true;
            }

            if(goToCarryBox && CarryOnLemon.CheckScore(lemonBox.lemons[clickedObj].score))
            {
                lemonBox.lemons[clickedObj].UI.gameObject.SetActive(false);

                if (CarryOnLemon.showScore == 0)
                {
                    nowScore += CarryOnLemon.setScore;
                    score.text = nowScore.ToString();
                    CarryOnLemon.SetScore(new Random().Next(7, 19));
                }
                CheckBox();
            }
            else
            {
                RectTransform checkSize = lemonBox.rect;

                Vector2 putOnClickPos = clickedObj.transform.localPosition;

                Vector2 overSize = new Vector2(Mathf.Abs(putOnClickPos.x) + lemonBox.lemons[clickedObj].size.x * 0.5f, Mathf.Abs(putOnClickPos.y) + lemonBox.lemons[clickedObj].size.y * 0.5f);

                if (overSize.x > checkSize.sizeDelta.x * 0.5f || overSize.y > checkSize.sizeDelta.y * 0.5f)
                {
                    clickedObj.transform.SetSiblingIndex(number);
                    clickedObj.transform.localPosition = exLocation;
                }
            }

            number = -1;
            exLocation = Vector2.zero;
            clickedObj = null;
            Clicked = false;
        }
    }

    public void clickBack()
    {
        if(GameState && timer < 30f) gameMgr.Createadd();
        gameOver.SetActive(false);
        gameObject.SetActive(false);
        startObjScript?.Invoke(this);
    }


    public void GameOver()
    {
        gameMgr.Createadd();
        gameOver.SetActive(true);
        bestScore.text = "score : " + nowScore;
        startObjScript?.Invoke(this);
    }
    public void ClickRestart()
    {
        gameOver.SetActive(false);
        GameState = true;
        SettingStart();
    }

    public GameObject ResultSlot(GameObject ben = null)
    {

        PointerEventData data = new(EventSystem.current);

        data.position = Input.mousePosition;

        List<RaycastResult> result = new();

        EventSystem.current.RaycastAll(data, result);

        if (result.Count > 0)
        {
            if (ben != null && result[0].gameObject == ben && result.Count > 1) return result[1].gameObject;  
            return result[0].gameObject;
        }
        else
        {
            return null;
        }
    }

}

public class Lemon
{
    public Lemon(GameObject ui)
    { 
        UI = ui; 
        size = ui.GetComponent<RectTransform>().sizeDelta;
    }

    public Vector2 size;
    public GameObject UI;
    public Text scoreText;
    public int score;
}
public class LemonBox
{
    public RectTransform rect;
    public Dictionary<GameObject, Lemon> lemons = new();

    public void SortLemon()
    {
        foreach (Lemon lemon in lemons.Values)
        {
            float x = new Random().Next((int)(rect.rect.xMin + lemon.size.x * 0.5f), (int)(rect.rect.xMax - lemon.size.x * 0.5f));
            float y = new Random().Next((int)(rect.rect.yMin + lemon.size.y * 0.5f), (int)(rect.rect.yMax -lemon.size.y * 0.5f));
            Vector2 pos = new Vector2(x, y);

            lemon.score = new Random().Next(1, 9);
            lemon.scoreText.text = lemon.score.ToString();
            lemon.UI.SetActive(true);
            lemon.UI.transform.localPosition = pos;
        }
    }
}

public class CarryOnLemon
{
    public GameObject UI;
    public int setScore;
    public int showScore;
    public Text scoreText;

    public void SetScore(int score)
    {
        setScore = score;
        showScore = setScore;
        scoreText.text = setScore.ToString();
    }

    public bool CheckScore(int score)
    {
        int scores = showScore;
        scores -= score;

        showScore = scores >= 0 ? scores : showScore;

        scoreText.text = showScore.ToString();

        return showScore == scores;
    }
}


//로그인 옮기기 , 랭킹, 시간초