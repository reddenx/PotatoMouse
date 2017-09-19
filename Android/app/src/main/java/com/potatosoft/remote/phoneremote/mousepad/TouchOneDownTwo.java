package com.potatosoft.remote.phoneremote.mousepad;

import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by Sean on 4/11/2016.
 */
public class TouchOneDownTwo extends MousePadStateBase {

    private Timer Expiration;
    private final int DOUBLE_CLICK_DRAG_TIMEOUT = 300;
    private final int DRAG_MOVE_THRESHOLD = 20;
    private float StartX;
    private float StartY;

    public TouchOneDownTwo(MousePadStateBase previousState, float x, float y) {
        super(previousState);

        StartX = x;
        StartY = y;

        Expiration = new Timer();
        Expiration.schedule(new TimerTask() {
            @Override
            public void run() {
                StartDragging();
            }
        }, DOUBLE_CLICK_DRAG_TIMEOUT);
    }

    private void StartDragging() {
        StateHandler.OutputDragStart();
        ChangeState(new LeftDrag(this, StartX, StartY));
    }

    @Override
    public void TouchUp(float x, float y) {
        Expiration.cancel();
        StateHandler.OutputDoubleClick();
        ChangeState(new ReadyState(this));
    }

    @Override
    public void TouchDown(float x, float y) {
        //ignore
    }

    @Override
    public void TouchMove(float x, float y) {
        if(Math.abs(StartX - x) > DRAG_MOVE_THRESHOLD || Math.abs(StartY - y) > DRAG_MOVE_THRESHOLD) {
            Expiration.cancel();
            StartDragging();
        }
    }
}
