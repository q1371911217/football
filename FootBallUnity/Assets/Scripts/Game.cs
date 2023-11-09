using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public enum EGameState
{
    WAIT,
    GAMING,
    SHOOTING,
    END,
}

public class Record
{
    string time;
    string mul;
    string win;
    public Record(string time, string mul, string win)
    {
        this.time = time;
        this.mul = mul;
        this.win = win;
    }

    public string getTime()
    {
        return time;
    }

    public string getMul()
    {
        return mul;
    }

    public string getWin()
    {
        return win;
    }

}

public class Game : MonoBehaviour
{

    public static bool isLoad = false;
    List<int> shootRate = new List<int>() { 50, 40, 30, 25, 20 };
    List<float> mulList = new List<float>() { 1.92f, 3.84f, 7.68f, 15.36f, 30.72f };

    List<float> amountList = new List<float>() { 0.145f, 0.335f, 0.535f, 0.76f, 1 };

    List<string> topList = new List<string>() { "Group stage: {0}/4 ", "Elimination game：{0}/8", "semifinal：{0}/4", "final:{0}/1" };

    List<Record> recordList = new List<Record>();

    [SerializeField]
    List<AudioClip> clipList;
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Text lblCoin;
    [SerializeField]
    Button btnHelp, btnVolum, btnBack, btnLeft, btnRight, btnPlay, btnCollect, btnRandom;

    [SerializeField]
    Transform svContent;
    [SerializeField]
    GameObject itemPref;

    [SerializeField]
    List<Transform> ballList;
    [SerializeField]
    List<Button> btnPointList;
    [SerializeField]
    Transform player, ball;

    [SerializeField]
    GameObject bgGoal, bgNoGoal;
    [SerializeField]
    Text lblGoal;
    [SerializeField]
    Image fgBar;
    [SerializeField]
    Text txtWin;
    [SerializeField]
    GameObject helpLayer;

    [SerializeField]
    Text lblTop, lblBottom;

    Vector3 ballStartPos;

    EGameState gameState;

    public static bool volumOpen = true;

    float curCoin = 10000.00f;
    int curBet = 100;  //25-800
    int shootTimes = 1;
    int shootGoalTimes = 0;

    int matchNum; //比赛场数
    int enemyScore;
    int selfScore;

    int winNum = 0;


    private void Start()
    {
        audioSource.volume = Game.volumOpen ? 1 : 0;
        btnVolum.transform.Find("spDisable").gameObject.SetActive(!volumOpen);

        gameState = EGameState.WAIT;
        ballStartPos = ball.transform.localPosition;
        for (int i = 0; i < btnPointList.Count; i++)
        {
            int tmp = i + 1;
            btnPointList[i].onClick.AddListener(delegate()
            {
                shoot(tmp);
            });
        }

        lblCoin.text = string.Format("{0:N2}", curCoin);
        updateBet();
    }

    void updateMoney(float addCoin)
    {
        curCoin += addCoin;
        if (curCoin < 1000)
            curCoin = 10000.00f;
        lblCoin.text = string.Format("{0:N2}", curCoin);
    }

    void updateBet()
    {
        int leftBet = curBet / 2;
        int middleBet = curBet;
        int rightBet = curBet * 2;

        foreach(Transform item in ballList)
        {
            item.Find("spBig").gameObject.SetActive(false);
        }

        if(leftBet < 25)
        {
            leftBet = 25;
            middleBet = leftBet * 2;
            rightBet = middleBet * 2;
            ballList[0].Find("spBig").gameObject.SetActive(true);
        }else if(rightBet > 800)
        {
            rightBet = 800;
            middleBet = rightBet / 2;
            leftBet = middleBet / 2;
            ballList[2].Find("spBig").gameObject.SetActive(true);
        }
        else
            ballList[1].Find("spBig").gameObject.SetActive(true);

        ballList[0].transform.Find("lblNum").GetComponent<Text>().text = leftBet.ToString();
        ballList[1].transform.Find("lblNum").GetComponent<Text>().text = middleBet.ToString();
        ballList[2].transform.Find("lblNum").GetComponent<Text>().text = rightBet.ToString();
        ballList[0].transform.Find("spBig/lblNum").GetComponent<Text>().text = leftBet.ToString();
        ballList[1].transform.Find("spBig/lblNum").GetComponent<Text>().text = middleBet.ToString();
        ballList[2].transform.Find("spBig/lblNum").GetComponent<Text>().text = rightBet.ToString();
    }

    void updateRecord()
    {
        for (int i = 0; i < recordList.Count; i++)
        {
            Transform cell;
            if (i < svContent.childCount)
            {
                cell = svContent.GetChild(i);
            }
            else
            {
                cell = GameObject.Instantiate(itemPref).transform;
                cell.gameObject.SetActive(true);
                cell.name = i.ToString();
                cell.SetParent(svContent);
                cell.localPosition = Vector3.zero;
                cell.localRotation = Quaternion.identity;
                cell.localScale = Vector3.one;
            }
            cell.Find("lblTime").GetComponent<Text>().text = recordList[i].getTime();
            cell.Find("lblWin").GetComponent<Text>().text = recordList[i].getWin().ToString();
            cell.Find("bgPower/lblPower").GetComponent<Text>().text = recordList[i].getMul();
        }
    }

    void gameStart()
    {
        matchNum++;

        if(matchNum < 5)
        {
            lblTop.text = string.Format(topList[0], matchNum);
        }else if(matchNum < 13)
        {
            lblTop.text = string.Format(topList[1], matchNum - 4);
        }else if(matchNum < 17)
        {
            lblTop.text = string.Format(topList[2], matchNum - 12);
        }
        else
        {
            lblTop.text = string.Format(topList[3], 1);
        }

        enemyScore = Random.Range(1, 5);
        selfScore = Random.Range(0, enemyScore);
        shootGoalTimes = selfScore;
        lblBottom.text = string.Format("Score:{0}/{1}", shootGoalTimes, enemyScore);

        if(shootGoalTimes > 0)
            fgBar.DOFillAmount(amountList[shootGoalTimes - 1], 0.3f);

        shootTimes = 0;
       // shootGoalTimes = 0;
        ball.gameObject.SetActive(true);
        btnCollect.transform.Find("spDisable").gameObject.SetActive(false);
        btnRandom.gameObject.SetActive(true);
        btnPlay.gameObject.SetActive(false);
        foreach (Button b in btnPointList)
        {
            b.gameObject.SetActive(true);
        }

        
    }

    void shoot(int index)
    {
        if (gameState != EGameState.GAMING) return;
        audioSource.PlayOneShot(clipList[6]);
        shootTimes++;
        gameState = EGameState.SHOOTING;
        int c = ((index + Random.Range(1, 4)) % 5) + 1;

        bool isOk = false;

        if (Random.Range(0, 101) < shootRate[shootTimes - 1])//reward
        {
            isOk = true;
            shootGoalTimes++;

        }
        
        ball.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f).SetDelay(0.2f);
        ball.transform.DOLocalMove(btnPointList[index - 1].transform.localPosition, 0.5f).SetDelay(0.2f).OnComplete(() =>
        {
                player.transform.GetComponent<Image>().enabled = false;
                if (isOk)//reward
                {
                    player.transform.Find(string.Format("p_{0}", c)).gameObject.SetActive(true);
                    
                //float turnWin = curBet * mulList[shootGoalTimes - 1];
                // bgGoal.gameObject.SetActive(true);
                // lblGoal.text = string.Format("WIN {0:N2}", turnWin.ToString());
                lblBottom.text = string.Format("Score:{0}/{1}", shootGoalTimes, enemyScore);
                if(shootGoalTimes > 0 && shootGoalTimes < amountList.Count)
                fgBar.DOFillAmount(amountList[shootGoalTimes - 1], 0.3f);
                    
                    audioSource.PlayOneShot(clipList[4]);
                }
                else
                {
                    player.transform.Find(string.Format("p_{0}", index)).gameObject.SetActive(true);
                   // bgNoGoal.gameObject.SetActive(true);
                    audioSource.PlayOneShot(clipList[3]);
                }
            
        });
        StartCoroutine(initShoot());
    }

    IEnumerator initShoot()
    {
        if(shootTimes >= 5 || shootGoalTimes > enemyScore)
        {
            shootTimes = 6;
            gameState = EGameState.END;
            if(shootGoalTimes > enemyScore)
            {
                winNum++;
            }
        }
        yield return new WaitForSeconds(1);
        if(shootTimes >= 5) {
            if (matchNum == 4 || matchNum == 12 || matchNum == 16 || matchNum == 17)
            {
                //Debug.LogError(winNum);
                if ((matchNum == 4 && winNum >= 2) || (matchNum == 12 && winNum >= 4) || (matchNum == 16 && winNum >= 2) || (matchNum == 17 && winNum >= 1))
                {
                    bgGoal.gameObject.SetActive(true);
                    if (matchNum == 17)
                    {
                        matchNum = 0;
                    }
                    winNum = 0;
                }
                else
                {
                    matchNum = 0;
                    winNum = 0;
                    bgNoGoal.gameObject.SetActive(true);
                }
            }
        }

        yield return new WaitForSeconds(1);
        if (gameState != EGameState.WAIT)
        {
            player.transform.GetComponent<Image>().enabled = true;
            bgNoGoal.gameObject.SetActive(false);
            bgGoal.gameObject.SetActive(false);
            for (int i = 1; i <= 5; i++)
            {
                player.transform.Find(string.Format("p_{0}", i)).gameObject.SetActive(false);
            }
            ball.localScale = Vector3.one;
            ball.transform.localPosition = ballStartPos;

            if (shootTimes >= 5)
            {
                ball.gameObject.SetActive(false);
                foreach (Button b in btnPointList)
                {
                    b.gameObject.SetActive(false);
                }
                float win = 0;
                //if (shootGoalTimes > 0)
                //{
                    //win = curBet * mulList[shootGoalTimes - 1];

                    //updateMoney(win);
                    Record record = new Record(lblTop.text, string.Format("score:{0}/{1}", selfScore, enemyScore), string.Format("{0}/{1}", shootGoalTimes, enemyScore));
                    recordList.Insert(0, record);
                    if (recordList.Count > 30)
                    {
                        recordList.RemoveAt(recordList.Count - 1);
                    }
                    updateRecord();
                //}
                btnRandom.gameObject.SetActive(false);
                btnPlay.gameObject.SetActive(true);
                btnCollect.transform.Find("spDisable").gameObject.SetActive(true);
                fgBar.fillAmount = 0;
                //if(win > 0)
                //{
                //    txtWin.transform.localPosition = new Vector3(-191, -50, 0);
                //    txtWin.text = string.Format("+{0:N2}", win);
                //    txtWin.transform.DOLocalMoveY(-20, 1f).OnComplete(()=> {
                //        txtWin.text = "";
                //    });
                //    audioSource.PlayOneShot(clipList[5]);
                //}
                

                gameState = EGameState.WAIT;
            }
            else
            {
                gameState = EGameState.GAMING;
            }
        }
    }

    public void btnMusic()
    {
        audioSource.PlayOneShot(clipList[1]);
    }
    public void onBtnClick(string name)
    {
        if(name == "btnHelp")
        {
            audioSource.PlayOneShot(clipList[1]);
            helpLayer.gameObject.SetActive(true);
        }
        else if(name == "btnVolum")
        {
            audioSource.PlayOneShot(clipList[1]);
            volumOpen = !volumOpen;
            btnVolum.transform.Find("spDisable").gameObject.SetActive(!volumOpen);
            audioSource.volume = volumOpen ? 1.0f : 0f;
        }
        else if(name == "btnBack")
        {
            audioSource.PlayOneShot(clipList[1]);
            SceneManager.LoadSceneAsync("LoginScene");
        }
        else if(name == "btnLeft")
        {
            audioSource.PlayOneShot(clipList[2]);
            int tmpBet = curBet / 2;
            if(tmpBet >= 25)
            {
                curBet = tmpBet;
                updateBet();
            }
        }
        else if(name == "btnRight")
        {
            audioSource.PlayOneShot(clipList[2]);
            int tmpBet = curBet * 2;
            if (tmpBet <= 800)
            {
                curBet = tmpBet;
                updateBet();
            }
        }
        else if(name == "btnPlay")
        {
            audioSource.PlayOneShot(clipList[1]);
            if (gameState == EGameState.WAIT)
            {
                if(curBet <= curCoin)
                {
                    gameState = EGameState.GAMING;
                    updateMoney(-curBet);
                    gameStart();
                }                
            }
        }
        else if(name == "btnRandom")
        {
            audioSource.PlayOneShot(clipList[1]);
            shoot(Random.Range(1, btnPointList.Count + 1));
        }
        else if(name == "btnCollect")
        {
            audioSource.PlayOneShot(clipList[1]);
        //    if (gameState == EGameState.GAMING  && shootTimes < 5)
        //    {
        //        ball.DOKill();
        //        ball.gameObject.SetActive(false);
        //        foreach (Button b in btnPointList)
        //        {
        //            b.gameObject.SetActive(false);
        //        }
        //        if (shootTimes == 0)
        //            updateMoney(curBet);

        //        if (shootGoalTimes > 0)
        //        {
        //            float win = curBet * mulList[shootGoalTimes - 1];
        //            updateMoney(win);
        //            Record record = new Record(System.DateTime.Now.ToString("HH:mm:ss"), string.Format("X{0:N2}", mulList[shootGoalTimes - 1]), win);
        //            recordList.Insert(0, record);
        //            if (recordList.Count > 30)
        //            {
        //                recordList.RemoveAt(recordList.Count - 1);
        //            }
        //            updateRecord();
        //            if (win > 0)
        //            {
        //                txtWin.transform.localPosition = new Vector3(-191, -50, 0);
        //                txtWin.text = string.Format("+{0:N2}", win);
        //                txtWin.transform.DOLocalMoveY(-20, 1f).OnComplete(() => {
        //                    txtWin.text = "";
        //                });
        //                audioSource.PlayOneShot(clipList[5]);
        //            }
        //        }
        //        btnRandom.gameObject.SetActive(false);
        //        btnPlay.gameObject.SetActive(true);
        //        btnCollect.transform.Find("spDisable").gameObject.SetActive(true);
        //        fgBar.fillAmount = 0;
                
        //        gameState = EGameState.WAIT;
        //    }
        }
       
    }
}
