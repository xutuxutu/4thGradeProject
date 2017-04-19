using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

public enum INPUT_EXCEPTION_STATE
{
    JUMP,
    TURN,
    USE_LADDER,
    PREVENT_LEFT,
    PREVENT_RIGHT,
}
// 최근
public class CInputMamager : MonoBehaviour
{
    public static CInputMamager instance { get; private set; }
    JoypadInput joypad;
    KeyboardInput keyboard;


    bool wasInputKeyboard;      // 최근 입력된 Input종류 ( ui표시나 키 변경시 )

    public CInputChangeUI inputUI;
    public float padDeadZone;   // 패드 민감도
    PadCode curPadInputs;       // 현재 입력받은 패드키 들
    private INPUTSTATE curInputStates;

    KeyCode[] keyboardCodes;
    KeyCode[] joypadCodes_nonAxis;
    PadCode[] joypadCodes_Axis;

    Dictionary<PadCode, bool> isPadGetKeyDown;
    Dictionary<INPUT_EXCEPTION_STATE, List<INPUTSTATE>> inputExceptionList;
    public List<INPUT_EXCEPTION_STATE> curInputException;

    public void fAwake()
    {
        instance = this;
    }
    public void fStart()
    {
        fNormalKeySetting();
        padDeadZone = 0.8f;

        // 어떤 키가 입력 되었는지 검사 할 때 모든 키코드를 검색하면 오래 걸리니
        // 입력될 수 있는 키들의 목록을 따로 만들어줌 
        KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
        keyboardCodes = new KeyCode[133];
        joypadCodes_nonAxis = new KeyCode[20];
        Array.Copy(keyCodes, 1, keyboardCodes, 0, 133);
        Array.Copy(keyCodes, 141, joypadCodes_nonAxis, 0, 20);

        // 패드도 위와 마찬가지
        PadCode[] padCodes = Enum.GetValues(typeof(PadCode)) as PadCode[];
        joypadCodes_Axis = new PadCode[14];
        Array.Copy(padCodes, 1, joypadCodes_Axis, 0, 14);


        isPadGetKeyDown = new Dictionary<PadCode, bool>();
        foreach (PadCode padcode in joypadCodes_Axis)
        {
            isPadGetKeyDown.Add(padcode, false);
        }

        fInputExceptionListInit();
        //fStartInputException(INPUT_EXCEPTION_STATE.TURN);
        //fEndInputException(INPUT_EXCEPTION_STATE.JUMP);
        wasInputKeyboard = true;
    }

    public void fUpdate()
    {
        fGetJoypadInput();
    }


    // 기본 키세팅
    void fNormalKeySetting()
    {
        keyboard.button_Left = KeyCode.LeftArrow;
        keyboard.button_Right = KeyCode.RightArrow;
        keyboard.button_Up= KeyCode.UpArrow;
        keyboard.button_Down = KeyCode.DownArrow;
        keyboard.button_Jump = KeyCode.Space;
        keyboard.button_Interact = KeyCode.LeftControl;
        keyboard.button_DeadRun = KeyCode.LeftShift;

        joypad.button_Left = PadCode.Axis1_Left;
        joypad.button_Right = PadCode.Axis1_Right;
        joypad.button_Up = PadCode.Axis1_Up;
        joypad.button_Down = PadCode.Axis1_Down;
        joypad.button_Jump = PadCode.A;
    }

    #region 키 입력 예외 처리
    // 키 입력을 막을 상황 초기화
    void fInputExceptionListInit()
    {
        curInputException = new List<INPUT_EXCEPTION_STATE>();
        inputExceptionList = new Dictionary<INPUT_EXCEPTION_STATE, List<INPUTSTATE>>();

        // 점프 중일 때
        fAdd2InputExceptionList(INPUT_EXCEPTION_STATE.JUMP, INPUTSTATE.LEFT, INPUTSTATE.RIGHT);

        // 방향 전환 할 때
        fAdd2InputExceptionList(INPUT_EXCEPTION_STATE.TURN, INPUTSTATE.LEFT, INPUTSTATE.RIGHT, INPUTSTATE.JUMP);

        // 왼쪽 입력을 막아야 할 때
        fAdd2InputExceptionList(INPUT_EXCEPTION_STATE.PREVENT_LEFT, INPUTSTATE.LEFT);

        // 오른쪽 입력을 막아야 할 때
        fAdd2InputExceptionList(INPUT_EXCEPTION_STATE.PREVENT_RIGHT, INPUTSTATE.RIGHT);

    }

    // 키 입력 예외 시작
    public void fStartInputException(INPUT_EXCEPTION_STATE excptionState)
    {
        if (!curInputException.Contains(excptionState))
            curInputException.Add(excptionState);
    }

    // 키 입력 예외 해제
    public void fEndInputException(INPUT_EXCEPTION_STATE excptionState)
    {
        if (curInputException.Contains(excptionState))
            curInputException.Remove(excptionState);
    }

    // 입력 예외 처리
    bool fIsInputExcption(INPUTSTATE inputState)
    {
        foreach (INPUT_EXCEPTION_STATE exception in curInputException)
        {
            if(inputExceptionList[exception].Contains(inputState))
                return true;
        }
        return false;
    }


    // 입력 예외 상황 리스트에 등록
    void fAdd2InputExceptionList(INPUT_EXCEPTION_STATE excptionState, params INPUTSTATE[] input)
    {
        if (!inputExceptionList.ContainsKey(excptionState))  // 처음 등록되는 이벤트의 경우에만 리스트 초기화 설정 해준다.
            inputExceptionList.Add(excptionState, new List<INPUTSTATE>());

        foreach (INPUTSTATE inputState in input)
            inputExceptionList[excptionState].Add(inputState);
        
    }
    #endregion


    #region 키 변경
    // 키 재설정
    public IEnumerator fReSettingKey(GameObject obj, bool isKeybord, string keyName)
    {
        if (isKeybord)
        {
            if (keyboard.button_Left.ToString() == keyName)
            {
                keyboard.button_Left = KeyCode.None;
                yield return StartCoroutine(fKeyboardInputForKeySetting(value => keyboard.button_Left = value));
                inputUI.ChangeButtonText(obj, keyboard.button_Left.ToString());
            }

            else if (keyboard.button_Right.ToString() == keyName)
            {
                keyboard.button_Right = KeyCode.None;
                yield return StartCoroutine(fKeyboardInputForKeySetting(value => keyboard.button_Right = value));
                inputUI.ChangeButtonText(obj, keyboard.button_Right.ToString());
            }
        }
        else
        {
            if (joypad.button_Left.ToString() == keyName)
            {
                joypad.button_Left = PadCode.None;
                yield return StartCoroutine(fGetJoypadInputForKeySetting(value => joypad.button_Left = value));
                inputUI.ChangeButtonText(obj, joypad.button_Left.ToString());
            }
            else if (joypad.button_Right.ToString() == keyName)
            {
                joypad.button_Right = PadCode.None;
                yield return StartCoroutine(fGetJoypadInputForKeySetting(value => joypad.button_Right = value));
                inputUI.ChangeButtonText(obj, joypad.button_Right.ToString());
            }
        }

        yield return null;
    }

    // 키보드 (등록된 키) 중복 체크
    bool fKeyboardSettingOverlapCheck(KeyCode checkKeyCode)
    {
        foreach (var keycode in typeof(KeyboardInput).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (keycode.GetValue(keyboard).Equals(checkKeyCode))
            {
                return true;
            }
        }
        return false;
    }

    // 조이패드 (등록된 키) 중복 체크
    bool fJoypadSettingOverlapCheck(PadCode checkPadCode)
    {
        foreach (var padcode in typeof(JoypadInput).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (padcode.GetValue(joypad).Equals(checkPadCode))
            {
                return true;
            }
        }
        return false;
    }

    // 키 세팅을 위한 키보드 입력
    public IEnumerator fKeyboardInputForKeySetting(Action<KeyCode> result)
    {
        while (true)
        {
            foreach (KeyCode keyCode in keyboardCodes)
            {
                if (Input.GetKeyDown(keyCode) && !fKeyboardSettingOverlapCheck(keyCode))
                {
                    result(keyCode);
                    yield break;

                }
            }
            yield return null;
        }
    }

    // 키 세팅을 위한 조이패드 입력
    public IEnumerator fGetJoypadInputForKeySetting(Action<PadCode> result)
    {
        while (true)
        {
            foreach (KeyCode padCode_nonAxis in joypadCodes_nonAxis)
            {
                if (Input.GetKeyDown(padCode_nonAxis))
                {
                    result(fKeycode2Padcode(padCode_nonAxis));
                    yield break;
                }
            }

            foreach (PadCode padCode_Axis in joypadCodes_Axis)
            {
                if (fInputPadGetKey(padCode_Axis) && !fJoypadSettingOverlapCheck(padCode_Axis))
                {
                    result(padCode_Axis);
                    yield break;
                }
            }
            yield return null;
        }
    }

    // 키 코드를 패드 코드로 변환
    PadCode fKeycode2Padcode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.JoystickButton0: return PadCode.A;
            case KeyCode.JoystickButton1: return PadCode.B;
            case KeyCode.JoystickButton2: return PadCode.X;
            case KeyCode.JoystickButton3: return PadCode.Y;
            case KeyCode.JoystickButton4: return PadCode.Bumper_Left;
            case KeyCode.JoystickButton5: return PadCode.Bumper_Right;
            case KeyCode.JoystickButton6: return PadCode.Back;
            case KeyCode.JoystickButton7: return PadCode.Start;
            case KeyCode.JoystickButton8: return PadCode.Axis1_Button;
            case KeyCode.JoystickButton9: return PadCode.Axis2_Button;
        }
        return PadCode.None;
    }
    #endregion


    #region 키 입력
    // 키보드 입력과 패드입력을 같이 처리 해줌 ( Input.GetKey 함수 역할 )
    public bool fGetKey(INPUTSTATE inputState)
    {
        if (fIsInputExcption(inputState))
            return false;

        switch (inputState)
        {
            case INPUTSTATE.LEFT:
                return Input.GetKey(keyboard.button_Left) || fInputPadGetKey(joypad.button_Left);
            case INPUTSTATE.RIGHT:
                return Input.GetKey(keyboard.button_Right) || fInputPadGetKey(joypad.button_Right);
            case INPUTSTATE.JUMP:
                return Input.GetKey(keyboard.button_Jump) || fInputPadGetKey(joypad.button_Jump);
            case INPUTSTATE.UP:
                return Input.GetKey(keyboard.button_Up) || fInputPadGetKey(joypad.button_Up);
            case INPUTSTATE.DOWN:
                return Input.GetKey(keyboard.button_Down) || fInputPadGetKey(joypad.button_Down);
            case INPUTSTATE.SHOOT:
                return Input.GetKey(keyboard.button_Shoot) || fInputPadGetKey(joypad.button_Shoot);
            case INPUTSTATE.RELOAD:
                return Input.GetKey(keyboard.button_Reload) || fInputPadGetKey(joypad.button_Reload);
            case INPUTSTATE.SNEAK:
                return Input.GetKey(keyboard.button_Sneak) || fInputPadGetKey(joypad.button_Sneak);
            case INPUTSTATE.DEADRUN:
                return Input.GetKey(keyboard.button_DeadRun) || fInputPadGetKey(joypad.button_DeadRun);
            case INPUTSTATE.INTERACT:
                return Input.GetKey(keyboard.button_Interact) || fInputPadGetKey(joypad.button_Interact);
            default:
                return false;
        }
    }

    // 키보드 입력과 패드입력을 같이 처리 해줌 ( Input.GetKeyDown 함수 역할 )
    public bool fGetKeyDown(INPUTSTATE inputState)
    {
        if (fIsInputExcption(inputState))
            return false;

        switch (inputState)
        {
            case INPUTSTATE.LEFT:
                return Input.GetKeyDown(keyboard.button_Left)     || fInputPadGetKeyDown(joypad.button_Left);
            case INPUTSTATE.RIGHT:                         
                return Input.GetKeyDown(keyboard.button_Right)    || fInputPadGetKeyDown(joypad.button_Right);
            case INPUTSTATE.JUMP:                          
                return Input.GetKeyDown(keyboard.button_Jump)     || fInputPadGetKeyDown(joypad.button_Jump);
            case INPUTSTATE.UP:                            
                return Input.GetKeyDown(keyboard.button_Up)       || fInputPadGetKeyDown(joypad.button_Up);
            case INPUTSTATE.DOWN:
                return Input.GetKeyDown(keyboard.button_Down)     || fInputPadGetKeyDown(joypad.button_Down);
            case INPUTSTATE.SHOOT:
                return Input.GetKeyDown(keyboard.button_Shoot)    || fInputPadGetKeyDown(joypad.button_Shoot);
            case INPUTSTATE.RELOAD:
                return Input.GetKeyDown(keyboard.button_Reload)   || fInputPadGetKeyDown(joypad.button_Reload);
            case INPUTSTATE.SNEAK:
                return Input.GetKeyDown(keyboard.button_Sneak)    || fInputPadGetKeyDown(joypad.button_Sneak);
            case INPUTSTATE.DEADRUN:
                return Input.GetKeyDown(keyboard.button_DeadRun)  || fInputPadGetKeyDown(joypad.button_DeadRun);
            case INPUTSTATE.INTERACT:
                return Input.GetKeyDown(keyboard.button_Interact) || fInputPadGetKeyDown(joypad.button_Interact);
            default:
                return false;
        }
    }

    // 키보드 입력과 패드입력을 같이 처리 해줌 ( Input.GetKeyUp 함수 역할 )
    public bool fGetKeyUp(INPUTSTATE inputState)
    {
        if (fIsInputExcption(inputState))
            return false;

        switch (inputState)
        {
            case INPUTSTATE.LEFT:
                return Input.GetKeyUp(keyboard.button_Left)     || fInputPadGetKeyUp(joypad.button_Left);
            case INPUTSTATE.RIGHT:                       
                return Input.GetKeyUp(keyboard.button_Right)    || fInputPadGetKeyUp(joypad.button_Right);
            case INPUTSTATE.JUMP:                        
                return Input.GetKeyUp(keyboard.button_Jump)     || fInputPadGetKeyUp(joypad.button_Jump);
            case INPUTSTATE.UP:                          
                return Input.GetKeyUp(keyboard.button_Up)       || fInputPadGetKeyUp(joypad.button_Up);
            case INPUTSTATE.DOWN:                        
                return Input.GetKeyUp(keyboard.button_Down)     || fInputPadGetKeyUp(joypad.button_Down);
            case INPUTSTATE.SHOOT:                       
                return Input.GetKeyUp(keyboard.button_Shoot)    || fInputPadGetKeyUp(joypad.button_Shoot);
            case INPUTSTATE.RELOAD:                      
                return Input.GetKeyUp(keyboard.button_Reload)   || fInputPadGetKeyUp(joypad.button_Reload);
            case INPUTSTATE.SNEAK:                       
                return Input.GetKeyUp(keyboard.button_Sneak)    || fInputPadGetKeyUp(joypad.button_Sneak);
            case INPUTSTATE.DEADRUN:
                return Input.GetKeyUp(keyboard.button_DeadRun)  || fInputPadGetKeyUp(joypad.button_DeadRun);
            case INPUTSTATE.INTERACT:
                return Input.GetKeyUp(keyboard.button_Interact) || fInputPadGetKeyUp(joypad.button_Interact);
            default:
                return false;
        }
    }

    // 어떤 키입력 받았는지 판단 ( 스틱, 트리거) 
    void fGetJoypadInput()
    {
        curPadInputs = PadCode.None;
        if (Input.GetAxis("JoyStick_Axis1") != 0)
        {
            
            if (Input.GetAxis("JoyStick_Axis1") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis1_Left;
            else if (Input.GetAxis("JoyStick_Axis1") > padDeadZone)
                curPadInputs |= PadCode.Axis1_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis2") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis2") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis1_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis2") > padDeadZone)
                curPadInputs |= PadCode.Axis1_Down;
        }

        if (Input.GetAxisRaw("JoyStick_Axis3") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis3") < padDeadZone * -1)
                curPadInputs |= PadCode.Trigger_Right;
            else if (Input.GetAxisRaw("JoyStick_Axis3") > padDeadZone)
                curPadInputs |= PadCode.Trigger_Left;
        }

        if (Input.GetAxisRaw("JoyStick_Axis4") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis4") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis2_Left;
            else if (Input.GetAxisRaw("JoyStick_Axis4") > padDeadZone)
                curPadInputs |= PadCode.Axis2_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis5") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis5") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis2_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis5") > padDeadZone)
                curPadInputs |= PadCode.Axis2_Down;
        }

        if (Input.GetAxisRaw("JoyStick_Axis6") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis6") < padDeadZone * -1)
                curPadInputs |= PadCode.DPad_Left;
            else if (Input.GetAxisRaw("JoyStick_Axis6") > padDeadZone)
                curPadInputs |= PadCode.DPad_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis7") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis7") < padDeadZone * -1)
                curPadInputs |= PadCode.DPad_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis7") > padDeadZone)
                curPadInputs |= PadCode.DPad_Down;
        }
    }

    // 패드 입력 ( Input.GetKey 함수 역할 )
    bool fInputPadGetKey(PadCode padCode)
    {
        if ((curPadInputs & PadCode.None) == padCode) return false;
        if ((curPadInputs & PadCode.Axis1_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Right) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Up) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Down) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Right) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Up) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Down) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Left) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Right) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Up) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Down) == padCode) return true;
        if ((curPadInputs & PadCode.Trigger_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Trigger_Right) == padCode) return true;

        if (Input.GetKey(KeyCode.JoystickButton0) && PadCode.A == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton1) && PadCode.B == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton2) && PadCode.X == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton3) && PadCode.Y == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton4) && PadCode.Bumper_Left == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton5) && PadCode.Bumper_Right == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton6) && PadCode.Back == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton7) && PadCode.Start == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton8) && PadCode.Axis1_Button == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton9) && PadCode.Axis2_Button == padCode) return true;

        return false;
    }

    // 패드 입력 ( Input.GetKeyDown 함수 역할 )
    bool fInputPadGetKeyDown(PadCode padCode)
    {
        if ((curPadInputs & PadCode.None) == padCode) return false;
        if ((curPadInputs & PadCode.Axis1_Left) == padCode && isPadGetKeyDown[PadCode.Axis1_Left] == false) { isPadGetKeyDown[PadCode.Axis1_Left] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis1_Left)); return true; }
        if ((curPadInputs & PadCode.Axis1_Right) == padCode && isPadGetKeyDown[PadCode.Axis1_Right] == false) { isPadGetKeyDown[PadCode.Axis1_Right] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis1_Right));  return true; }
        if ((curPadInputs & PadCode.Axis1_Up) == padCode && isPadGetKeyDown[PadCode.Axis1_Up] == false) { isPadGetKeyDown[PadCode.Axis1_Up] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis1_Up)); return true; }
        if ((curPadInputs & PadCode.Axis1_Down) == padCode && isPadGetKeyDown[PadCode.Axis1_Down] == false) { isPadGetKeyDown[PadCode.Axis1_Down] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis1_Down));  return true; }
        if ((curPadInputs & PadCode.Axis2_Left) == padCode && isPadGetKeyDown[PadCode.Axis2_Left] == false) { isPadGetKeyDown[PadCode.Axis2_Left] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis2_Left));  return true; }
        if ((curPadInputs & PadCode.Axis2_Right) == padCode && isPadGetKeyDown[PadCode.Axis2_Right] == false) { isPadGetKeyDown[PadCode.Axis2_Right] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis2_Right));  return true; }
        if ((curPadInputs & PadCode.Axis2_Up) == padCode && isPadGetKeyDown[PadCode.Axis2_Up] == false) { isPadGetKeyDown[PadCode.Axis2_Up] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis2_Up));  return true; }
        if ((curPadInputs & PadCode.Axis2_Down) == padCode && isPadGetKeyDown[PadCode.Axis2_Down] == false) { isPadGetKeyDown[PadCode.Axis2_Down] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Axis2_Down));  return true; }
        if ((curPadInputs & PadCode.DPad_Left) == padCode && isPadGetKeyDown[PadCode.DPad_Left] == false) { isPadGetKeyDown[PadCode.DPad_Left] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.DPad_Left));  return true; }
        if ((curPadInputs & PadCode.DPad_Right) == padCode && isPadGetKeyDown[PadCode.DPad_Right] == false) { isPadGetKeyDown[PadCode.DPad_Right] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.DPad_Right));  return true; }
        if ((curPadInputs & PadCode.DPad_Up) == padCode && isPadGetKeyDown[PadCode.DPad_Up] == false) { isPadGetKeyDown[PadCode.DPad_Up] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.DPad_Up));  return true; }
        if ((curPadInputs & PadCode.DPad_Down) == padCode && isPadGetKeyDown[PadCode.DPad_Down] == false) { isPadGetKeyDown[PadCode.DPad_Down] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.DPad_Down));  return true; }
        if ((curPadInputs & PadCode.Trigger_Left) == padCode && isPadGetKeyDown[PadCode.Trigger_Left] == false) { isPadGetKeyDown[PadCode.Trigger_Left] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Trigger_Left));  return true; }
        if ((curPadInputs & PadCode.Trigger_Right) == padCode && isPadGetKeyDown[PadCode.Trigger_Right] == false) { isPadGetKeyDown[PadCode.Trigger_Right] = true; StartCoroutine(fPadAxisButtonUpChack(PadCode.Trigger_Right)); return true; }

        if (Input.GetKeyDown(KeyCode.JoystickButton0) && PadCode.A == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton1) && PadCode.B == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton2) && PadCode.X == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton3) && PadCode.Y == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton4) && PadCode.Bumper_Left == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton5) && PadCode.Bumper_Right == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton6) && PadCode.Back == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton7) && PadCode.Start == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton8) && PadCode.Axis1_Button == padCode) return true;
        if (Input.GetKeyDown(KeyCode.JoystickButton9) && PadCode.Axis2_Button == padCode) return true;

        return false;
    }

    // 패드 입력 ( Input.GetKeyUp 함수 역할 )
    bool fInputPadGetKeyUp(PadCode padCode)
    {
        if ((curPadInputs & PadCode.None) == padCode) return false;
        if ((curPadInputs & PadCode.Axis1_Left)     != padCode && isPadGetKeyDown[PadCode.Axis1_Left] == true)    { isPadGetKeyDown[PadCode.Axis1_Left] = false; return true; }
        if ((curPadInputs & PadCode.Axis1_Right)    != padCode && isPadGetKeyDown[PadCode.Axis1_Right] == true)   { isPadGetKeyDown[PadCode.Axis1_Right] = false; return true; }
        if ((curPadInputs & PadCode.Axis1_Up)       != padCode && isPadGetKeyDown[PadCode.Axis1_Up] == true)      { isPadGetKeyDown[PadCode.Axis1_Up] = false; return true; }
        if ((curPadInputs & PadCode.Axis2_Left)     != padCode && isPadGetKeyDown[PadCode.Axis2_Left] == true)    { isPadGetKeyDown[PadCode.Axis2_Left] = false; return true; }
        if ((curPadInputs & PadCode.Axis2_Right)    != padCode && isPadGetKeyDown[PadCode.Axis2_Right] == true)   { isPadGetKeyDown[PadCode.Axis2_Right] = false; return true; }
        if ((curPadInputs & PadCode.Axis2_Up)       != padCode && isPadGetKeyDown[PadCode.Axis2_Up] == true)      { isPadGetKeyDown[PadCode.Axis2_Up] = false; return true; }
        if ((curPadInputs & PadCode.Axis2_Down)     != padCode && isPadGetKeyDown[PadCode.Axis2_Down] == true)    { isPadGetKeyDown[PadCode.Axis2_Down] = false; return true; }                                                                                                            
        if ((curPadInputs & PadCode.DPad_Left)      != padCode && isPadGetKeyDown[PadCode.DPad_Left] == true)     { isPadGetKeyDown[PadCode.DPad_Left] = false; return true; }
        if ((curPadInputs & PadCode.DPad_Right)     != padCode && isPadGetKeyDown[PadCode.DPad_Right] == true)    { isPadGetKeyDown[PadCode.DPad_Right] = false; return true; }
        if ((curPadInputs & PadCode.DPad_Up)        != padCode && isPadGetKeyDown[PadCode.DPad_Up] == true)       { isPadGetKeyDown[PadCode.DPad_Up] = false; return true; }
        if ((curPadInputs & PadCode.DPad_Down)      != padCode && isPadGetKeyDown[PadCode.DPad_Down] == true)     { isPadGetKeyDown[PadCode.DPad_Down] = false; return true; }
        if ((curPadInputs & PadCode.Trigger_Left)   != padCode && isPadGetKeyDown[PadCode.Trigger_Left] == true)  { isPadGetKeyDown[PadCode.Trigger_Left] = false; return true; }
        if ((curPadInputs & PadCode.Trigger_Right)  != padCode && isPadGetKeyDown[PadCode.Trigger_Right] == true) { isPadGetKeyDown[PadCode.Trigger_Right] = false; return true; }

        if (Input.GetKeyUp(KeyCode.JoystickButton0) && PadCode.A == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton1) && PadCode.B == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton2) && PadCode.X == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton3) && PadCode.Y == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton4) && PadCode.Bumper_Left == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton5) && PadCode.Bumper_Right == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton6) && PadCode.Back == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton7) && PadCode.Start == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton8) && PadCode.Axis1_Button == padCode) return true;
        if (Input.GetKeyUp(KeyCode.JoystickButton9) && PadCode.Axis2_Button == padCode) return true;

        return false;
    }

    // 패드 axis 입력의 경우 GetKeyDown을 한 후 키를 Up했을 때 처리
    IEnumerator fPadAxisButtonUpChack(PadCode padCode)
    {
        while (true)
        {
            if ((curPadInputs & padCode) != padCode)
            {
                isPadGetKeyDown[padCode] = false;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }


    #endregion

}



[Flags]
public enum PadCode
{
    None = 0,
    Axis1_Left = 1 << 0,
    Axis1_Right = 1 << 1,
    Axis1_Up = 1 << 2,
    Axis1_Down = 1 << 3,
    Axis2_Left = 1 << 4,
    Axis2_Right = 1 << 5,
    Axis2_Up = 1 << 6,
    Axis2_Down = 1 << 7,
    DPad_Left = 1 << 8,
    DPad_Right = 1 << 9,
    DPad_Up = 1 << 10,
    DPad_Down = 1 << 11,
    Trigger_Left = 1 << 12,
    Trigger_Right = 1 << 13,
    A = 1 << 14,
    B = 1 << 15,
    X = 1 << 16,
    Y = 1 << 17,
    Back = 1 << 18,
    Start = 1 << 19,
    Bumper_Left = 1 << 20,
    Bumper_Right = 1 << 21,
    Axis1_Button = 1 << 22,
    Axis2_Button = 1 << 23,

}

struct JoypadInput
{
    public PadCode button_Left;
    public PadCode button_Right;
    public PadCode button_Up;
    public PadCode button_Down;
    public PadCode button_Jump;
    public PadCode button_Shoot;
    public PadCode button_Reload;
    public PadCode button_Sneak;
    public PadCode button_DeadRun;
    public PadCode button_Interact;
}

struct KeyboardInput
{
    public KeyCode button_Left;
    public KeyCode button_Right;
    public KeyCode button_Up;
    public KeyCode button_Down;
    public KeyCode button_Jump;
    public KeyCode button_Shoot;
    public KeyCode button_Reload;
    public KeyCode button_Sneak;
    public KeyCode button_DeadRun;
    public KeyCode button_Interact;
}

public enum INPUTSTATE
{
    NONE = 0,
    LEFT = 1 << 0,
    RIGHT = 1 << 1,
    UP = 1 << 2,
    DOWN = 1 << 3,
    JUMP = 1 << 4,
    SHOOT = 1 << 5,
    RELOAD = 1 << 6,
    SNEAK = 1 << 7,
    DEADRUN = 1 << 8,
    INTERACT = 1 << 9,
}



// 이전 코드
/*
public class InputMamager : MonoBehaviour {

    JoypadInput joypad;
    KeyboardInput keyboard;
    GameInputCommand inputCmd;

    bool wasInputKeyboard;      // 최근 입력된 Input종류 ( ui표시나 키 변경시 )

    public Actor actor1;
    public Actor actor2;
    Actor curActor;             // 현재 조종 대상

    public InputChangeUI inputUI;
    public float padDeadZone;   // 패드 민감도
    PadCode curPadInputs;       // 현재 입력받은 패드키 들

    KeyCode[] keyboardCodes;  
    KeyCode[] joypadCodes_nonAxis;
    PadCode[] joypadCodes_Axis;

    KeyCode tempInputCode;

    public void fAwake()
    {
        inputCmd.button_Left = new LeftCommend();
        inputCmd.button_Right = new RightCommend();
        inputCmd.button_Up = new UpCommend();
        inputCmd.button_Down = new DownCommend();
        inputCmd.button_Jump = new JumpCommend();
    }

    public void fStart()
    {
        NormalKeySetting();

        KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
        keyboardCodes = new KeyCode[133];
        joypadCodes_nonAxis = new KeyCode[20];
        Array.Copy(keyCodes, 1, keyboardCodes, 0, 133);
        Array.Copy(keyCodes, 141, joypadCodes_nonAxis, 0, 20);

        PadCode[] padCodes = Enum.GetValues(typeof(PadCode)) as PadCode[];
        joypadCodes_Axis = new PadCode[12];
        Array.Copy(padCodes, 1, joypadCodes_Axis, 0, 12);

//        curActor = actor1;
        wasInputKeyboard = true;
    }

    public void fUpdate()
    {
        GetJoypadInput();
        InputHandle();
    }


    // 기본 키세팅
    void NormalKeySetting()
    {
        keyboard.button_Left = KeyCode.LeftArrow;
        keyboard.button_Right = KeyCode.RightArrow;
        keyboard.button_switch = KeyCode.Alpha1;
        keyboard.button_Jump = KeyCode.Space;


        joypad.button_Left = PadCode.DPad_Left;
        joypad.button_Right = PadCode.DPad_Right;
        joypad.button_switch = PadCode.Back;

    }

    // 키 재설정
    public IEnumerator ReSettingKey(GameObject obj, bool isKeybord, string keyName)
    {
        if (isKeybord)
        {
            if (keyboard.button_Left.ToString() == keyName)
            {
                keyboard.button_Left = KeyCode.None;
                yield return StartCoroutine(KeyboardInputForKeySetting(value => keyboard.button_Left = value));
                inputUI.ChangeButtonText(obj, keyboard.button_Left.ToString());
            }

            else if (keyboard.button_Right.ToString() == keyName)
            {
                keyboard.button_Right = KeyCode.None;
                yield return StartCoroutine(KeyboardInputForKeySetting(value => keyboard.button_Right = value));
                inputUI.ChangeButtonText(obj, keyboard.button_Right.ToString());
            }
            else if (keyboard.button_switch.ToString() == keyName)
            {
                keyboard.button_switch = KeyCode.None;
                yield return StartCoroutine(KeyboardInputForKeySetting(value => keyboard.button_switch = value));
                inputUI.ChangeButtonText(obj, keyboard.button_switch.ToString());
            }
        }
        else
        {
            if (joypad.button_Left.ToString() == keyName)
            {
                joypad.button_Left = PadCode.None;
                yield return StartCoroutine(GetJoypadInputForKeySetting(value => joypad.button_Left = value));
                inputUI.ChangeButtonText(obj, joypad.button_Left.ToString());
            }
            else if (joypad.button_Right.ToString() == keyName)
            {
                joypad.button_Right = PadCode.None;
                yield return StartCoroutine(GetJoypadInputForKeySetting(value => joypad.button_Right = value));
                inputUI.ChangeButtonText(obj, joypad.button_Right.ToString());
            }
            else if (joypad.button_switch.ToString() == keyName)
            {
                joypad.button_switch = PadCode.None;
                yield return StartCoroutine(GetJoypadInputForKeySetting(value => joypad.button_switch = value));
                inputUI.ChangeButtonText(obj, joypad.button_switch.ToString());
            }
        }

        yield return null;
    }

    // 키보드 (등록된 키) 중복 체크
    bool KeyboardSettingOverlapCheck(KeyCode checkKeyCode)
    {
        foreach (var keycode in typeof(KeyboardInput).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (keycode.GetValue(keyboard).Equals(checkKeyCode))
            {
                return true;
            }
        }
        return false;
    }

    // 조이패드 (등록된 키) 중복 체크
    bool JoypadSettingOverlapCheck(PadCode checkPadCode)
    {
        foreach (var padcode in typeof(JoypadInput).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (padcode.GetValue(joypad).Equals(checkPadCode))
            {
                return true;
            }
        }
        return false;
    }

    // 키 세팅을 위한 키보드 입력
    public IEnumerator KeyboardInputForKeySetting(Action<KeyCode> result)
    {
        while (true)
        {
            foreach (KeyCode keyCode in keyboardCodes)
            {
                if (Input.GetKeyDown(keyCode) && !KeyboardSettingOverlapCheck(keyCode))
                {
                    result(keyCode);
                    yield break;

                }
            }
            yield return null;
        }
    }

    // 키 세팅을 위한 조이패드 입력
    public IEnumerator GetJoypadInputForKeySetting(Action<PadCode> result)
    {
        while (true)
        {
            foreach (KeyCode padCode_nonAxis in joypadCodes_nonAxis)
            {
                if (Input.GetKeyDown(padCode_nonAxis))
                {
                    result(Keycode2Padcode(padCode_nonAxis));
                    yield break;
                }
            }

            foreach (PadCode padCode_Axis in joypadCodes_Axis)
            {
                if (InputPadGetKey(padCode_Axis) && !JoypadSettingOverlapCheck(padCode_Axis))
                {
                    result(padCode_Axis);
                    yield break;
                }
            }
            yield return null;
        }
    }

    // 키코드를 패드코드로 변환
    PadCode Keycode2Padcode(KeyCode keyCode)
    {
        switch(keyCode)
        {
            case KeyCode.JoystickButton0 : return PadCode.A;
            case KeyCode.JoystickButton1 : return PadCode.B;
            case KeyCode.JoystickButton2 : return PadCode.X;
            case KeyCode.JoystickButton3 : return PadCode.Y;
            case KeyCode.JoystickButton4 : return PadCode.Bumper_Left;
            case KeyCode.JoystickButton5 : return PadCode.Bumper_Right;
            case KeyCode.JoystickButton6 : return PadCode.Back;
            case KeyCode.JoystickButton7 : return PadCode.Start;
            case KeyCode.JoystickButton8 : return PadCode.Axis1_Button;
            case KeyCode.JoystickButton9 : return PadCode.Axis2_Button;
        }
        return PadCode.None;
    }

    // 키 입력 처리
    void InputHandle()
    {
        
        if (Input.GetKeyDown(keyboard.button_switch) || InputPadGetKey(joypad.button_switch))
        {
            wasInputKeyboard = Input.GetKeyDown(keyboard.button_switch);
            
            if (curActor == actor1)
                curActor = actor2;
            else
                curActor = actor1;
            
        }
        

        if (Input.GetKey(keyboard.button_Left)  || InputPadGetKey(joypad.button_Left))
        {
            wasInputKeyboard = Input.GetKey(keyboard.button_Left);
            //inputCmd.button_Left.Excute(curActor);
            inputCmd.button_Left.Excute(actor1);
        }

        if (Input.GetKey(keyboard.button_Right) || InputPadGetKey(joypad.button_Right))
        {
            wasInputKeyboard = Input.GetKey(keyboard.button_Right);

            inputCmd.button_Right.Excute(actor1);
            //inputCmd.button_Right.Excute(curActor);
        }

        if (Input.GetKey(keyboard.button_Jump) || InputPadGetKey(joypad.button_Jump))
        {
            wasInputKeyboard = Input.GetKey(keyboard.button_Jump);

            inputCmd.button_Jump.Excute(actor1);
            //inputCmd.button_Jump.Excute(curActor);
        }

    }

    // 어떤 키입력 받았는지 판단 ( 스틱, 트리거) 
    void GetJoypadInput()
    {
        curPadInputs = PadCode.None;
        if (Input.GetAxis("JoyStick_Axis1") != 0)
        {
            if (Input.GetAxis("JoyStick_Axis1") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis1_Left;
            else if (Input.GetAxis("JoyStick_Axis1") > padDeadZone)
                curPadInputs |= PadCode.Axis1_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis2") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis2") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis1_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis2") > padDeadZone)
                curPadInputs |= PadCode.Axis1_Down;
        }

        if (Input.GetAxisRaw("JoyStick_Axis3") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis3") < padDeadZone * -1)
                curPadInputs |= PadCode.Trigger_Right;
            else if (Input.GetAxisRaw("JoyStick_Axis3") > padDeadZone)
                curPadInputs |= PadCode.Trigger_Left;
        }

        if (Input.GetAxisRaw("JoyStick_Axis4") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis4") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis2_Left;
            else if (Input.GetAxisRaw("JoyStick_Axis4") > padDeadZone)
                curPadInputs |= PadCode.Axis2_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis5") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis5") < padDeadZone * -1)
                curPadInputs |= PadCode.Axis2_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis5") > padDeadZone)
                curPadInputs |= PadCode.Axis2_Down;
        }

        if (Input.GetAxisRaw("JoyStick_Axis6") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis6") < padDeadZone * -1)
                curPadInputs |= PadCode.DPad_Left;
            else if (Input.GetAxisRaw("JoyStick_Axis6") > padDeadZone)
                curPadInputs |= PadCode.DPad_Right;
        }

        if (Input.GetAxisRaw("JoyStick_Axis7") != 0)
        {
            if (Input.GetAxisRaw("JoyStick_Axis7") < padDeadZone * -1)
                curPadInputs |= PadCode.DPad_Up;
            else if (Input.GetAxisRaw("JoyStick_Axis7") > padDeadZone)
                curPadInputs |= PadCode.DPad_Down;
        }
    }

    // 패드 입력 ( Input.GetKeyDown 함수 역할 )
    bool InputPadGetKey(PadCode padCode)
    {
        if ((curPadInputs & PadCode.None) == padCode) return false;
        if ((curPadInputs & PadCode.Axis1_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Right) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Up) == padCode) return true;
        if ((curPadInputs & PadCode.Axis1_Down) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Right) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Up) == padCode) return true;
        if ((curPadInputs & PadCode.Axis2_Down) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Left) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Right) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Up) == padCode) return true;
        if ((curPadInputs & PadCode.DPad_Down) == padCode) return true;
        if ((curPadInputs & PadCode.Trigger_Left) == padCode) return true;
        if ((curPadInputs & PadCode.Trigger_Right) == padCode) return true;
        
        if (Input.GetKey(KeyCode.JoystickButton0) && PadCode.A == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton1) && PadCode.B == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton2) && PadCode.X == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton3) && PadCode.Y == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton4) && PadCode.Bumper_Left == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton5) && PadCode.Bumper_Right == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton6) && PadCode.Back == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton7) && PadCode.Start == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton8) && PadCode.Axis1_Button == padCode) return true;
        if (Input.GetKey(KeyCode.JoystickButton9) && PadCode.Axis2_Button == padCode) return true;

        return false;
    }
}


[Flags]
public enum PadCode
{
    None = 0,
    Axis1_Left    = 1 << 0,
    Axis1_Right   = 1 << 1,
    Axis1_Up      = 1 << 2,
    Axis1_Down    = 1 << 3,
    Axis2_Left    = 1 << 4,
    Axis2_Right   = 1 << 5,
    Axis2_Up      = 1 << 6,
    Axis2_Down    = 1 << 7,
    DPad_Left     = 1 << 8,
    DPad_Right    = 1 << 9,
    DPad_Up       = 1 << 10,
    DPad_Down     = 1 << 11,
    Trigger_Left  = 1 << 12,
    Trigger_Right = 1 << 13,
    A             = 1 << 14,
    B             = 1 << 15,
    X             = 1 << 16,
    Y             = 1 << 17,
    Back          = 1 << 18,
    Start         = 1 << 19,
    Bumper_Left   = 1 << 20,
    Bumper_Right  = 1 << 21,
    Axis1_Button  = 1 << 22,
    Axis2_Button  = 1 << 23,

}

struct JoypadInput
{

    public PadCode button_switch;

    public PadCode button_Left;
    public PadCode button_Right;
    public PadCode button_Up;
    public PadCode button_Down;
    public PadCode button_Jump;
    public PadCode button_Jump;
    public PadCode button_Shoot;
    public PadCode button_Reload;
    public PadCode button_Sneak;
    public PadCode button_DeadRun;
}

struct KeyboardInput
{
    public KeyCode button_switch;

    public KeyCode button_Left;
    public KeyCode button_Right;
    public KeyCode button_Up;
    public KeyCode button_Down;
    public KeyCode button_Jump;
    public KeyCode button_Jump;
    public KeyCode button_Shoot;
    public KeyCode button_Reload;
    public KeyCode button_Sneak;
    public KeyCode button_DeadRun;
}

struct GameInputCommand
{
    public ICommend button_Left;
    public ICommend button_Right;
    public ICommend button_Up;
    public ICommend button_Down;
    public ICommend button_Jump;
    public ICommend button_Jump;
    public ICommend button_Shoot;
    public ICommend button_Reload;
    public ICommend button_Sneak;
    public ICommend button_DeadRun;
}                           
                            
#region 입력 커맨드 정리 (ICommend)
public interface ICommend
{
    void Excute(Actor actor);
}

public class LeftCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.MoveLeft();
    }
}

public class RightCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.MoveRight();
    }
}

public class UpCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.MoveUp();
    }
}

public class DownCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.MoveDown();
    }
}

public class JumpCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.Jump();
    }
}

public class ShootCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.Shoot();
    }
}

public class ReloadCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.Reload();
    }
}

public class SneakCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.Sneak();
    }
}

public class DeadRunCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.DeadRun();
    }
}

public class InteractionCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.Interaction();
    }
}

public class UseItemCommend : ICommend
{
    public void Excute(Actor actor)
    {
        actor.UseItem();
    }
}
#endregion
*/