using System;
using System.Text.RegularExpressions;
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
    private List<string> possibleSolutions = new List<string>();

    public TextMesh targetText;
    private int target = 1; //just in case, not 0 because it would auto-solve

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
    private int possibleSolutionsAvailable = 0;

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
        do
        {
            target = UnityEngine.Random.Range(100,1000);
            GenerateLargeNumbers();
            GenerateNumbers();
            Main();
        }
        while (possibleSolutionsAvailable == 0);
        Debug.LogFormat("[Cruel Countdown #{0}] Your numbers are {1}, {2}, {3}, {4}, {5} & {6}.", moduleId, selectedNumbers[0], selectedNumbers[1], selectedNumbers[2], selectedNumbers[3], selectedNumbers[4], selectedNumbers[5]);
        Debug.LogFormat("[Cruel Countdown #{0}] Your target is {1}.", moduleId, target);
        Debug.LogFormat("[Cruel Countdown #{0}] List of possible solutions: {1}.", moduleId, string.Join(" || ", possibleSolutions.ToArray()));
        targetText.text = target.ToString();
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
        selectedLarge.Clear();
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
            // Note from K.S.: Don't attempt to divide by zero, TP will catch the DivideByZeroException and autosolve us
            if(secondPress.chosenNumber == 0)
            {
                Debug.LogFormat("[Cruel Countdown #{0}] Strike! You can't divide by zero.", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
                return;
            }
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
            mostRecentSolve = 0;
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
    
    //THE SOLVER
    internal interface IExpression { }; 
    
    class Op : IExpression
    {
        public Func<int, int, int> Func { get; private set; }
        public string Name { get; private set; }

        public Op(string name, Func<int, int, int> op)
        {
            Func = op;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class Num : IExpression
    {
        public int Val { get; set; }

        public Num(int val) { Val = val; }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    private readonly Op[] Ops =
    {
        new Op("+", (a, b) => a + b),
        new Op("-", (a, b) => a - b),
        new Op("*", (a, b) => a * b),
        new Op("/", (a, b) =>
        {
            if (a%b != 0)
            {
                throw new ArgumentException();
            }
            return a/b;
        })
    };

    void Main()
    {
        var targetNumber = target;

        var stack = new Stack<IExpression>();

        var numbers = selectedNumbers.ToList();

        NextGen(numbers, stack, targetNumber);
    }

    private void NextGen(List<int> numbers, Stack<IExpression> stack, int target)
    {
        for (int i = 0; i < numbers.Count; i++)
        {
            var n = numbers[i];
            var nextList = new List<int>(numbers);
            nextList.RemoveAt(i);

            stack.Push(new Num(n));

            try
            {
                var val = Evaluate(stack);
                if (val == target)
                {
                    Print(stack);
                }

                if (val < 0) throw new Exception();

                foreach (var op in Ops)
                {
                    stack.Push(op);
                    NextGen(nextList, stack, target);
                    stack.Pop();
                }
            }
            catch
            { }

            stack.Pop();
        }
    }

    private void Print(IEnumerable<IExpression> stack)
    {
        string Equation = "";
        bool FirstNumberPassed = false;
        foreach (var expression in stack.Reverse())
        {
            int Out;
            Equation = Int32.TryParse(expression.ToString(), out Out) == true && FirstNumberPassed ? Equation.Insert(0, "(") : Equation;
            Equation = Equation + expression.ToString();
            Equation = Int32.TryParse(expression.ToString(), out Out) == true && FirstNumberPassed ? Equation + ")" : Equation;
            FirstNumberPassed = true;
        }
        Equation = Equation.Substring(1);
        Equation = Equation.Remove(Equation.Length - 1);
        possibleSolutions.Add(Equation);
        possibleSolutionsAvailable++;
    }

    private int Evaluate(IEnumerable<IExpression> stack)
    {
        int acc = 0;
        Op currentOp = null;
        foreach (var expression in stack.Reverse())
        {
            if (expression is Num)
            {
                int val = (expression as Num).Val;

                acc = currentOp != null ? currentOp.Func(acc, val) : val;
            }
            else
            {
                currentOp = expression as Op;
            }
        }
        return acc;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // Twitch Plays implementation handled by Kaito Sinclaire (K_S_)
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use '!{0} activate' or '!{0} go' to start the clock, then '!{0} 2 * 8', '!{0} 25 / 5', etc. Commands are chainable using semicolons.";
#pragma warning restore 414

    public IEnumerator ProcessTwitchCommand(string command)
    {
        Match mt;
        String errorStr;
        List<string> cmds = command.Split(';').ToList();
        bool anyCommandValid = false;

        foreach (string cmd in cmds)
        {
            if (Regex.IsMatch(cmd, @"^\s*(?:press|select)?\s*(?:activate|go|start|clock)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;

                // If the clock's already been pressed, just ignore it.
                // Don't consider it an error, just move along.
                if (clockOn)
                    continue;

                Debug.LogFormat("[Cruel Countdown #{0}] TP Command: Your next line is... \"Here's the countdown clock...\"", moduleId);

                // If time runs out, the person who starts the clock first gets the strike.
                yield return "strike";
                yield return new KMSelectable[] { clock };
            }
            else if ((mt = Regex.Match(cmd, @"^\s*(?:press|select)?\s*(\d{1,4})\s*([+\-*×x/÷])\s*(\d{1,4})\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
            {
                // Match group 1: First number
                // Match group 2: Operator
                // Match group 3: Second number
                int numberA = Convert.ToInt32(mt.Groups[1].ToString());
                int numberB = Convert.ToInt32(mt.Groups[3].ToString());
                string operatorTx = mt.Groups[2].ToString();
                KMSelectable numberSelA = null, numberSelB = null, operSel = null;

                if (!clockOn)
                {
                    yield return "sendtochaterror The clock hasn't been started! You need to do that first.";
                    yield break;
                }

                foreach(ClickableNumbers number in numbers)
                {
                    if (number.position != 10 && number.chosenNumber == numberA)
                    {
                        numberSelA = number.selectable;
                        break;
                    }
                }
                if (numberSelA == null)
                {
                    errorStr = String.Format("sendtochaterror I couldn't find a {1} to use for '{0}'. Stopped at that point.",
                        cmd, numberA);
                    yield return errorStr;
                    yield break;
                }

                foreach(ClickableNumbers number in numbers)
                {
                    // Obviously we can't pick the same selectable again.
                    if (number.selectable != numberSelA && number.position != 10 && number.chosenNumber == numberB)
                    {
                        numberSelB = number.selectable;
                        break;
                    }
                }
                if (numberSelB == null)
                {
                    errorStr = String.Format("sendtochaterror I couldn't find a {2}{1} to use for '{0}'. Stopped at that point.",
                        cmd, numberB, (numberA == numberB) ? "second " : "");
                    yield return errorStr;
                    yield break;
                }

                if (operatorTx.Equals("+"))
                {
                    Debug.LogFormat("[Cruel Countdown #{0}] TP Command: {1} plus {2}", moduleId, numberA, numberB);
                    operSel = operators[0];
                }
                else if (operatorTx.Equals("-"))
                {
                    Debug.LogFormat("[Cruel Countdown #{0}] TP Command: {1} minus {2}", moduleId, numberA, numberB);
                    operSel = operators[1];
                }
                else if (operatorTx.Equals("/") || operatorTx.Equals("÷"))
                {
                    Debug.LogFormat("[Cruel Countdown #{0}] TP Command: {1} divided by {2}", moduleId, numberA, numberB);
                    operSel = operators[3];
                }
                else // X, x, *, ×
                {
                    Debug.LogFormat("[Cruel Countdown #{0}] TP Command: {1} times {2}", moduleId, numberA, numberB);
                    operSel = operators[2];
                }

                yield return null;
                yield return new KMSelectable[] { numberSelA, operSel, numberSelB };
            }
            else
            {
                if (anyCommandValid)
                {
                    errorStr = String.Format("sendtochaterror I don't recognize '{0}'. Stopped at that point.", cmd);
                    yield return errorStr;
                }
                yield break;
            }
            anyCommandValid = true;
            yield return new WaitForSeconds(0.25f);
        }
        yield break;
    }

    void TwitchHandleForcedSolve()
    {
        GetComponent<KMBombModule>().HandlePass();
        moduleSolved = true;
        Debug.LogFormat("[Cruel Countdown #{0}] Twitch Plays requested a solve.", moduleId);
        if (clockOn)
        {
            // Act like we've just been solved the normal way
            clockAnimation.enabled = false;
            foreach(KMSelectable op in operators)
                op.GetComponentInChildren<TextMesh>().color = textColours[0];
            for(int i = 0; i <= 5; i++)
            {
                numbers[i].numberText.text = selectedNumbers[i].ToString();
                numbers[i].numberText.color = textColours[0];
            }

            clockSFX.Stop();
            Audio.PlaySoundAtTransform("bell", transform);
        }
    }
}
