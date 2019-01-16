using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class cruelCountdownScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public AudioSource clockSFX;
    public ClickableNumbers[] numbers;
    public KMSelectable clock;
    public KMSelectable[] operators;

    public Animator clockAnimation;
    public Renderer secondHand;
    private int[] largeNumbers = new int[4];
    private bool clockOn;

    private int[] selectedNumbers = new int[6];
    private List<int> selectedLarge = new List<int>();

    public TextMesh targetText;
    private int target;
    private int chosenEquation = 0;

    private ClickableNumbers firstPress;
    private ClickableNumbers secondPress;
    private int pressedPosition = 9;
    private int secondPressPosition = 0;
    private bool operatorAdded;
    private string selectedOperation = "";
    public Color[] textColours;

    public TextMesh[] boardWriting;
    private int equationsDone = 0;
    private int boardFirst = 0;
    private int mostRecentSolve = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Update()
    {
        if(!moduleSolved && mostRecentSolve == target)
        {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Cruel Countdown #{0}] You have made {1}. Module solved.", moduleId, target);
            clockAnimation.enabled = false;
            for(int i = 0; i <= 5; i++)
            {
                numbers[i].numberText.text = selectedNumbers[i].ToString();
                numbers[i].numberText.color = textColours[0];
            }
            clockSFX.Stop();
            Audio.PlaySoundAtTransform("bell", transform);
        }
    }

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (ClickableNumbers number in numbers)
        {
            ClickableNumbers pressedNumber = number;
            number.selectable.OnInteract += delegate () { NumberPress(pressedNumber); return false; };
        }
        foreach (KMSelectable operatorButton in operators)
        {
            KMSelectable pressedOperator = operatorButton;
            operatorButton.OnInteract += delegate () { OperatorPress(pressedOperator); return false; };
        }
        clock.OnInteract += delegate () { StartClock(); return false; };
    }


    void Start()
    {
        GenerateLargeNumbers();
        GenerateNumbers();
        GenerateTarget();
        Logging();
    }

    void GenerateLargeNumbers()
    {
        largeNumbers[0] = UnityEngine.Random.Range(81, 100);
        largeNumbers[1] = UnityEngine.Random.Range(61, 80);
        largeNumbers[2] = UnityEngine.Random.Range(41, 60);
        largeNumbers[3] = UnityEngine.Random.Range(21, 40);
    }
    void GenerateNumbers()
    {
        equationsDone = 0;
        boardFirst = 0;
        foreach(TextMesh equation in boardWriting)
        {
            equation.text = "";
        }
        int index = UnityEngine.Random.Range(1,8);
        index = (index % 4) + 1;
        for(int i = 0; i < index; i++)
        {
            int index2 = UnityEngine.Random.Range(0,4);
            while(selectedLarge.Contains(index2))
            {
                index2 = UnityEngine.Random.Range(0,4);
            }
            selectedLarge.Add(index2);
            selectedLarge.Sort();
        }
        for(int i = 0; i < index; i++)
        {
            numbers[i].chosenNumber = largeNumbers[selectedLarge[i]];
            numbers[i].numberText.text = largeNumbers[selectedLarge[i]].ToString();
        }
        for(int i = index; i < 6; i++)
        {
            int index3 = UnityEngine.Random.Range(1,16);
            numbers[i].chosenNumber = index3;
            numbers[i].numberText.text = index3.ToString();
        }
        for(int i = 0; i < 6; i++)
        {
            selectedNumbers[i] = numbers[i].chosenNumber;
        }
        for(int i = 0; i <= 5; i++)
        {
            numbers[i].position = i;
        }
        Debug.LogFormat("[Cruel Countdown #{0}] Your numbers are {1}, {2}, {3}, {4}, {5} & {6}.", moduleId, selectedNumbers[0], selectedNumbers[1], selectedNumbers[2], selectedNumbers[3], selectedNumbers[4], selectedNumbers[5]);
    }

    void GenerateTarget()
    {
        chosenEquation = UnityEngine.Random.Range(0,15);
        if(chosenEquation == 0)
        {
            target = (selectedNumbers[5] * selectedNumbers[1]) + (selectedNumbers[0] - selectedNumbers[4]) + (selectedNumbers[2] - selectedNumbers[3]);
        }
        else if(chosenEquation == 1)
        {
            target = (selectedNumbers[0] - selectedNumbers[1] + selectedNumbers[5]);
            if(selectedNumbers[2] > selectedNumbers[3])
            {
                target *= (selectedNumbers[2] - selectedNumbers[3]);
            }
            else if(selectedNumbers[3] > selectedNumbers[2])
            {
                target *= (selectedNumbers[3] - selectedNumbers[2]);
            }
            else
            {
                target += (selectedNumbers[3] + selectedNumbers[2]);
            }
        }
        else if(chosenEquation == 2)
        {
            target = ((selectedNumbers[0] + selectedNumbers[4] + selectedNumbers[2]) - selectedNumbers[1]) + (selectedNumbers[3] * selectedNumbers[5]);
        }
        else if(chosenEquation == 3)
        {
            target = (selectedNumbers[5] * selectedNumbers[1]) + (selectedNumbers[4] * selectedNumbers[3]) - (selectedNumbers[0] + selectedNumbers[2]);
        }
        else if(chosenEquation == 4)
        {
            target = ((selectedNumbers[1] - selectedNumbers[3] - selectedNumbers[4]) * selectedNumbers[5]) + selectedNumbers[0] - selectedNumbers[2];
        }
        else if (chosenEquation == 5)
        {
            target = (selectedNumbers[1] + selectedNumbers[0]) - (selectedNumbers[2] + selectedNumbers[3]) + (selectedNumbers[4] * selectedNumbers[5]);
        }
        else if (chosenEquation == 6)
        {
            target = (selectedNumbers[0] * selectedNumbers[4]) + (selectedNumbers[1] - selectedNumbers[5]) - (selectedNumbers[2] + selectedNumbers[3]);
        }
        else if (chosenEquation == 7)
        {
            target = ((selectedNumbers[5] + selectedNumbers[4]) * (selectedNumbers[2] - selectedNumbers[3]) + selectedNumbers[0] - selectedNumbers[1]);
        }
        else if (chosenEquation == 8)
        {
            target = ((selectedNumbers[2] * (selectedNumbers[4] + selectedNumbers[5])) - selectedNumbers[3]) + selectedNumbers[0];
        }
        else if (chosenEquation == 9)
        {
            target = ((selectedNumbers[1] + selectedNumbers[5] - selectedNumbers[2]) * selectedNumbers[0]) + selectedNumbers[4] - selectedNumbers[3];
        }
        else if (chosenEquation == 10)
        {
            target = (selectedNumbers[2] - selectedNumbers[3]) * (selectedNumbers[4] + selectedNumbers[5]);
        }
        else if (chosenEquation == 11)
        {
            target = (selectedNumbers[0] * selectedNumbers[5]) + selectedNumbers[4] + selectedNumbers[3];
        }
        else if (chosenEquation == 12)
        {
            target = (selectedNumbers[5] * selectedNumbers[4] * selectedNumbers[3]) - (selectedNumbers[0] + selectedNumbers[2]);
        }
        else if (chosenEquation == 13)
        {
            target = ((selectedNumbers[1] + selectedNumbers[5]) * selectedNumbers[4]) - selectedNumbers[3];
        }
        else if (chosenEquation == 14)
        {
            target = ((selectedNumbers[1] - selectedNumbers[4]) + (selectedNumbers[0] - selectedNumbers[2])) * selectedNumbers[5];
        }

        if(target > 1000 || target < 100)
        {
            GenerateTarget();
        }
    }

    void Logging()
    {
        targetText.text = target.ToString();
        Debug.LogFormat("[Cruel Countdown #{0}] Your target is {1}.", moduleId, target);
        if(chosenEquation == 0)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} × {2}) + ({3} - {4}) + ({5} - {6}).", moduleId, selectedNumbers[5], selectedNumbers[1], selectedNumbers[0], selectedNumbers[4], selectedNumbers[2], selectedNumbers[3]);
        }
        else if(chosenEquation == 1)
        {
            if(selectedNumbers[2] > selectedNumbers[3])
            {
                Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} - {2} + {3}) × ({4} - {5}).", moduleId, selectedNumbers[0], selectedNumbers[1], selectedNumbers[5], selectedNumbers[2], selectedNumbers[3]);
            }
            else if(selectedNumbers[3] > selectedNumbers[2])
            {
                Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} - {2} + {3}) × ({4} - {5}).", moduleId, selectedNumbers[0], selectedNumbers[1], selectedNumbers[5], selectedNumbers[3], selectedNumbers[2]);
            }
            else
            {
                Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} - {2} + {3}) × ({4} + {5}).", moduleId, selectedNumbers[0], selectedNumbers[1], selectedNumbers[5], selectedNumbers[3], selectedNumbers[2]);
            }
        }
        else if(chosenEquation == 2)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} + {2} + {3}) - {4}) + ({5} × {6}).", moduleId, selectedNumbers[0], selectedNumbers[4], selectedNumbers[2], selectedNumbers[1], selectedNumbers[3], selectedNumbers[5]);
        }
        else if(chosenEquation == 3)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} x {2}) + ({3} × {4}) - ({5} + {6}).", moduleId, selectedNumbers[5], selectedNumbers[1], selectedNumbers[4], selectedNumbers[3], selectedNumbers[0], selectedNumbers[2]);
        }
        else if(chosenEquation == 4)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} - {2} - {3}) × {4}) + {5} - {6}.", moduleId, selectedNumbers[1], selectedNumbers[3], selectedNumbers[4], selectedNumbers[5], selectedNumbers[0], selectedNumbers[2]);
        }
        else if(chosenEquation == 5)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} + {2}) - ({3} + {4}) + ({5} × {6}).", moduleId, selectedNumbers[1], selectedNumbers[0], selectedNumbers[2], selectedNumbers[3], selectedNumbers[4], selectedNumbers[5]);
        }
        else if(chosenEquation == 6)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} × {2}) + {3} - {4} - ({5} + {6}).", moduleId, selectedNumbers[0], selectedNumbers[4], selectedNumbers[1], selectedNumbers[5], selectedNumbers[2], selectedNumbers[3]);
        }
        else if(chosenEquation == 7)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} + {2}) × ({3} - {4}) + {5} - {6}.", moduleId, selectedNumbers[5], selectedNumbers[4], selectedNumbers[2], selectedNumbers[3], selectedNumbers[0], selectedNumbers[1]);
        }
        else if(chosenEquation == 8)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} × ({2} + {3})) - {4}) + {5}.", moduleId, selectedNumbers[2], selectedNumbers[4], selectedNumbers[5], selectedNumbers[3], selectedNumbers[0]);
        }
        else if(chosenEquation == 9)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} + {2} - {3}) × {4}) + {5} - {6}.", moduleId, selectedNumbers[1], selectedNumbers[5], selectedNumbers[2], selectedNumbers[0], selectedNumbers[4], selectedNumbers[3]);
        }
        else if(chosenEquation == 10)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} - {2}) × ({3} + {4}).", moduleId, selectedNumbers[2], selectedNumbers[3], selectedNumbers[4], selectedNumbers[5]);
        }
        else if(chosenEquation == 11)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} × {2}) + {3} + {4}.", moduleId, selectedNumbers[0], selectedNumbers[5], selectedNumbers[4], selectedNumbers[3]);
        }
        else if(chosenEquation == 12)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: ({1} × {2} × {3}) - ({4} + {5}).", moduleId, selectedNumbers[5], selectedNumbers[4], selectedNumbers[3], selectedNumbers[0], selectedNumbers[2]);
        }
        else if(chosenEquation == 13)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} + {2}) × {3}) - {4}.", moduleId, selectedNumbers[1], selectedNumbers[5], selectedNumbers[4], selectedNumbers[3]);
        }
        else if(chosenEquation == 14)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] One possible solution: (({1} - {2}) + ({3} - {4})) × {5}.", moduleId, selectedNumbers[1], selectedNumbers[4], selectedNumbers[0], selectedNumbers[2], selectedNumbers[5]);
        }
    }

    void NumberPress(ClickableNumbers number)
    {
        if(moduleSolved || number.position == 10 || !clockOn || number.position == pressedPosition)
        {
            return;
        }
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        number.selectable.AddInteractionPunch();
        if(!operatorAdded)
        {
            foreach(ClickableNumbers allNumbers in numbers)
            {
                allNumbers.numberText.color = textColours[0];
            }
            firstPress = number;
            pressedPosition = number.position;
            number.numberText.color = textColours[1];
        }
        else
        {
            secondPress = number;
            secondPressPosition = number.position;
            executeOperation();
        }
    }

    void OperatorPress(KMSelectable operatorButton)
    {
        if(moduleSolved || operatorAdded || firstPress == null || !clockOn)
        {
            return;
        }
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        operatorButton.AddInteractionPunch();
        if(operatorButton.GetComponentInChildren<TextMesh>().text == "+")
        {
            selectedOperation = "+";
        }
        else if(operatorButton.GetComponentInChildren<TextMesh>().text == "−")
        {
            selectedOperation = "−";
        }
        else if(operatorButton.GetComponentInChildren<TextMesh>().text == "×")
        {
            selectedOperation = "×";
        }
        else if(operatorButton.GetComponentInChildren<TextMesh>().text == "÷")
        {
            selectedOperation = "÷";
        }
        operatorAdded = true;
        operatorButton.GetComponentInChildren<TextMesh>().color = textColours[1];
    }

    void executeOperation()
    {
        boardFirst = firstPress.chosenNumber;
        if(selectedOperation == "+")
        {
            if(firstPress.chosenNumber + secondPress.chosenNumber > 9999)
            {
                Debug.LogFormat("[Cruel Countdown #{0}] Strike! {1} + {2} would yield a number greater than 9,999.", moduleId, firstPress.chosenNumber, secondPress.chosenNumber);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
                return;
            }
            numbers[pressedPosition].chosenNumber = firstPress.chosenNumber + secondPress.chosenNumber;
        }
        else if(selectedOperation == "−")
        {
            if(firstPress.chosenNumber - secondPress.chosenNumber < 0)
            {
                Debug.LogFormat("[Cruel Countdown #{0}] Strike! {1} - {2} would yield a negative number.", moduleId, firstPress.chosenNumber, secondPress.chosenNumber);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
                return;
            }
            numbers[pressedPosition].chosenNumber = firstPress.chosenNumber - secondPress.chosenNumber;
        }
        else if(selectedOperation == "×")
        {
            if(firstPress.chosenNumber * secondPress.chosenNumber > 9999)
            {
                Debug.LogFormat("[Cruel Countdown #{0}] Strike! {1} × {2} would yield a number greater than 9,999.", moduleId, firstPress.chosenNumber, secondPress.chosenNumber);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
                return;
            }
            numbers[pressedPosition].chosenNumber = firstPress.chosenNumber * secondPress.chosenNumber;
        }
        else if(selectedOperation == "÷")
        {
            if(firstPress.chosenNumber % secondPress.chosenNumber != 0 || firstPress.chosenNumber / secondPress.chosenNumber <= 0)
            {
                Debug.LogFormat("[Cruel Countdown #{0}] Strike! {1} ÷ {2} would yield a non-integer.", moduleId, firstPress.chosenNumber, secondPress.chosenNumber);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
                return;
            }
            numbers[pressedPosition].chosenNumber = firstPress.chosenNumber / secondPress.chosenNumber;
        }
        boardWriting[equationsDone].text = boardFirst + " " + selectedOperation + " " + secondPress.chosenNumber + " = " +  numbers[pressedPosition].chosenNumber;
        equationsDone++;
        mostRecentSolve = numbers[pressedPosition].chosenNumber;
        numbers[pressedPosition].numberText.text = numbers[pressedPosition].chosenNumber.ToString();
        numbers[secondPressPosition].chosenNumber = 0;
        numbers[secondPressPosition].numberText.text = "";
        numbers[secondPressPosition].selectable.enabled = false;
        numbers[secondPressPosition].position = 10;
        mostRecentSolve = numbers[pressedPosition].chosenNumber;
        Reset();
    }

    public void StartClock()
    {
        if(!clockOn)
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            clock.AddInteractionPunch();
            StartCoroutine(clockRoutine());
            clockOn = true;
            Debug.LogFormat("[Cruel Countdown #{0}] Here's the countdown clock...", moduleId);
        }
        else
        {
            return;
        }
    }

    IEnumerator clockRoutine()
    {
        clockSFX.Play();
        clockAnimation.SetBool("clockOn", true);
        yield return new WaitForSeconds(1f);
        clockAnimation.SetBool("clockOn", false);
        yield return new WaitForSeconds(30f);
        if(!moduleSolved)
        {
            Debug.LogFormat("[Cruel Countdown #{0}] Strike! You have run out of time.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            clockAnimation.SetBool("restart", true);
            selectedLarge.Clear();
            clockOn = false;
            Reset();
            Start();
            yield return new WaitForSeconds(2f);
            clockAnimation.SetBool("restart", false);
        }
    }

    void Reset()
    {
        firstPress = null;
        secondPress = null;
        pressedPosition = 9;
        secondPressPosition = 0;
        operatorAdded = false;
        selectedOperation = "";
        boardFirst = 0;
        foreach(ClickableNumbers number in numbers)
        {
            number.numberText.color = textColours[0];
        }
        foreach(KMSelectable op in operators)
        {
            op.GetComponentInChildren<TextMesh>().color = textColours[0];
        }
    }
}
