package com.potatosoft.remote.phoneremote.mousepad;

/**
 * Created by Sean on 4/11/2016.
 */
public class MoveState extends MousePadStateBase {

    private float PreviousX;
    private float PreviousY;

    public MoveState(MousePadStateBase previousState, float x, float y) {
        super(previousState);
        PreviousX = x;
        PreviousY = y;
    }

    @Override
    public void TouchUp(float x, float y) {
        ChangeState(new ReadyState(this));
    }

    @Override
    public void TouchDown(float x, float y) {
        //ignore
    }

    @Override
    public void TouchMove(float x, float y) {
        //move mouse

        int diffX = (int)( PreviousX - x );
        int diffY = (int)( PreviousY - y );

        if(Math.abs(diffX) > 0 || Math.abs(diffY) > 0) {
            PreviousX -= diffX;
            PreviousY -= diffY;

            StateHandler.OutputMove(-diffX, -diffY);
        }
    }
}
