package com.potatosoft.remote.phoneremote.mousepad;

import com.potatosoft.remote.phoneremote.Utilities.Publisher;
import com.potatosoft.remote.phoneremote.Utilities.Vector2;

/**
 * Created by seanm_000 on 9/18/2017.
 */

public class MousepadStateHandler {

    public Publisher<Vector2<Integer>> OnMouseMoved;
    public Publisher<Void> OnLeftClick;
    public Publisher<Void> OnLeftDoubleClick;
    public Publisher<Void> OnLeftUp;
    public Publisher<Void> OnLeftDown;

    private MousePadStateBase CurrentState;

    public MousepadStateHandler() {
        OnMouseMoved = new Publisher<>();
        OnLeftClick = new Publisher<>();
        OnLeftDoubleClick = new Publisher<>();
        OnLeftUp = new Publisher<>();
        OnLeftDown = new Publisher<>();

        CurrentState = new ReadyState(this);
    }

    public void SetState(MousePadStateBase newState) {
        this.CurrentState = newState;
    }

    public void InputTouchDown(float x, float y){
        CurrentState.TouchDown(x, y);
    }

    public void InputTouchUp(float x, float y){
        CurrentState.TouchUp(x, y);
    }

    public void InputTouchPositionChanged(float x, float y) {
        CurrentState.TouchMove(x, y);
    }


    //suppose this relay was redundant... but this api existed before the event subscription and is already consumed by the states
    public void OutputMove(int x, int y) {
        OnMouseMoved.Notify(new Vector2<>(x, y));
    }

    public void OutputDragStop() {
        OnLeftUp.Notify(null);
    }

    public void OutputDragStart() {
        OnLeftDown.Notify(null);
    }

    public void OutputDoubleClick() {
        OnLeftDoubleClick.Notify(null);
    }

    public void OutputClick() {
        OnLeftClick.Notify(null);
    }
}
