import { Connection } from './socket'

/** time to wait after the first click for a second to start dragging or doubleclick */
const SINGLE_CLICK_WAIT = 200;
/** distance moved before a single touch starts to be considered a move */
const TOUCH_MOVE_THRESHOLD = 10;
/** time a first touch is held in place to consider it a hesitant move gesture */
const TOUCH_MOVE_TIME_THRESHOLD = 400;
/** time to wait while holding a double click to start a drag, if lifted before this, it's considered a double click*/
const TOUCH_DRAG_TIME_THRESHOLD = 200;


export class Mousepad {

    /**
     * 
     * @param {Connection} connection 
     */
    constructor(connection) {
        var initialFakeState = {
            pad: this,
            notifier: new InternalMouseEventNotifier(connection),
            deactivate: () => { }
        }
        this.currentState = initialFakeState;
        this.setState(new ReadyState(initialFakeState));
    }

    setState(newState) {
        // console.log('moving from ' + this.currentState.name + ' to ' + newState.name);
        newState.activate();
        this.currentState.deactivate();
        this.currentState = newState;
    }
    inputTouchDown(x, y, id) {
        this.currentState.touchDown(x, y, id);
    }
    inputTouchUp(x, y, id) {
        this.currentState.touchUp(x, y, id);
    }
    inputTouchPositionChanged(x, y, id) {
        this.currentState.touchMove(x, y, id);
    }
}

function CMD(commandType, data) {
    return JSON.stringify({
        type: commandType,
        data: data
    })
}
const COMMANDS = {
    MOVE: 'mouseMove',
    CLICK: 'mouseClick',
    DBLCLICK: 'mouseDoubleClick',
    BUTTON_DOWN: 'mouseDown',
    BUTTON_UP: 'mouseUp',
    SCROLL_UP: 'scrollUp',
    SCROLL_DOWN: 'scrollDown'
}
class InternalMouseEventNotifier {
    /**
     * 
     * @param {Connection} connection 
     */
    constructor(connection) {
        this.connection = connection;
    }

    /**
     * 
     * @param {Number} x 
     * @param {Number} y 
     */
    move(x, y) {
        this.connection.send(CMD(COMMANDS.MOVE, x + ',' + y));
    }
    leftClick() {
        this.connection.send(CMD(COMMANDS.CLICK, 'left'));
    }
    leftDoubleClick() {
        this.connection.send(CMD(COMMANDS.DBLCLICK, 'left'));
    }
    leftDown() {
        this.connection.send(CMD(COMMANDS.BUTTON_DOWN, 'left'));
    }
    leftUp() {
        this.connection.send(CMD(COMMANDS.BUTTON_UP, 'left'));
    }
    scrollUp() {
        this.connection.send(CMD(COMMANDS.SCROLL_UP, '_'));
    }
    scrollDown() {
        this.connection.send(CMD(COMMANDS.SCROLL_DOWN, '_'));
    }
}


class MousepadBase {
    /**
     * 
     * @param {MousepadBase} previousState 
     */
    constructor(previousState) {
        /** @type {InternalMouseEventNotifier} */
        this.notifier = previousState.notifier;
        /** @type {MousePad} */
        this.pad = previousState.pad
        this.active = false;

        this.name = 'MousepadBase';
    }

    touchUp(x, y, id) {
        // console.log("MousepadBase::touchUp");
    }
    touchDown(x, y, id) {
        // console.log("MousepadBase::touchDown");
    }
    touchMove(x, y, id) {
        // console.log("MousepadBase::touchMove");
    }

    activate() {
        this.active = true;
    }
    deactivate() {
        this.active = false;
    }
}


/**
 * Ready state no touch detected, all things route back to this
 * transitions to:
 * - TouchOneDown when first touch happens
 */
class ReadyState extends MousepadBase {
    /**
     * 
     * @param {MousepadBase} previous 
     */
    constructor(previous) {
        super(previous);
        this.name = 'ReadyState';
    }

    touchDown(x, y, id) {
        // console.log("ReadyState::touchDown");
        if (this.active) {
            this.pad.setState(new TouchOneDown(this, x, y, id));
        }
    }
    // touchUp(x, y, id) { }
    // touchMove(x, y, id) { }
}

/**
 * from Ready after a single touch is detected
 * from here it can go to:
 * - TouchOneUp if it's released to prep for clicking, double clicking, or click dragging
 * - Moving after time has passed or has moved past threshold
 * - TouchTwoDown for two finger scrolling
 */
class TouchOneDown extends MousepadBase {
    constructor(previous, x1Start, y1Start, id1) {
        super(previous);

        this.x1Start = x1Start;
        this.y1Start = y1Start;
        this.id1 = id1;

        this.name = 'TouchOneDown';
    }

    activate() {
        super.activate();

        setTimeout(() => {
            if (this.active) {
                this.pad.setState(new Moving(this, this.x1Start, this.y1Start, this.id1));
            }
        }, TOUCH_MOVE_TIME_THRESHOLD);
    }

    touchDown(x, y, id) {
        if (this.active && this.id1 != id) {
            //TODO this was disabled to figure out scrolling/right click branch of state machine logic
            //this.pad.setState(new TouchTwoDown(this, this.x1Start, this.y1Start, this.id1));
        }
    }

    touchUp(x, y, id) {
        if (this.active) {
            this.pad.setState(new TouchOneUp(this));
        }
    }

    touchMove(x, y, id) {
        //check for distance moved, if over threshold set to moving state
        var distX = Math.abs(this.x1Start - x);
        var distY = Math.abs(this.y1Start - y);
        if (Math.max(distX, distY) > TOUCH_MOVE_THRESHOLD && this.active) {
            this.pad.setState(new Moving(this, this.x1Start, this.y1Start, id));
        }
    }
}

/**
 * two fingers are on the pad right now
 * from TouchOneDown, a second touch detected, this is used for scrolling
 * from here it can go to:
 * - Moving if one finger is released
 * - if released within threshhold, fire off right click before transitioning to Moving
 */
class TouchTwoDown extends MousepadBase {
    constructor(previous, x1, y1, id1, x2, y2, id2) {
        super(previous);
        this.name = 'TouchTwoDown';

        this.x1Start = x1;
        this.x2Start = x2;
        this.y1Start = y1;
        this.y2Start = y2;
        this.id1 = id1;
        this.id2 = id2;
    }

    activate() {
        super.activate();
        // this.setTimeout()
    }
    // touchDown(x, y, id) {
    //     //
    // }
    touchUp(x, y, id) {
        if (this.active) {
            if (this.id1 == id) {
                this.pad.setState(new Moving(this, this.x2Start, this.y2Start, this.id2));
            } else if (this.id2 == id) {
                this.pad.setState(new Moving(this, this.x1Start, this.y1Start, this.id1));
            }
        }
    }
    touchMove(x, y, id) {
    }
}

/**
 * From many, here any movement is sent as a move command
 * can transition to:
 * - TouchTwoDown if another finger is pressed
 * - Ready if the finger is released
 */
class Moving extends MousepadBase {
    constructor(previous, x1, y1, id1) {
        super(previous);
        this.name = 'Moving';
        this.x1Prev = x1;
        this.y1Prev = y1;
        this.id1 = id1;
    }

    touchDown(x, y, id) {
        if (id != this.id1 && this.active) {
            //TODO this was disabled to figure out scrolling/right click branch of state machine logic
            //this.pad.setState(new TouchTwoDown(this, this.x1Start, this.y1Start, this.id1, x, y, id));
        }
    }
    touchUp(x, y, id) {
        if (id == this.id1 && this.active) {
            this.pad.setState(new ReadyState(this));
        }
    }
    touchMove(x, y, id) {
        if (id == this.id1 && this.active) {
            var diffx = this.x1Prev - x;
            var diffy = this.y1Prev - y;
            this.notifier.move(-diffx, -diffy)
            this.x1Prev = x;
            this.y1Prev = y;
        }
    }
}

/**
 * Here we wait for a given time to see if we should fire a click event and return to Ready or if pressed again start a click drag
 * can transition to:
 * - TouchOneDownTwo if a press happens again
 * - Ready if a time passes, fire a click event
 */
class TouchOneUp extends MousepadBase {
    constructor(previous) {
        super(previous);
        this.name = 'TouchOneUp';
    }

    activate() {
        super.activate();

        setTimeout(() => {
            if (this.active) {
                this.pad.setState(new ReadyState(this));
                this.notifier.leftClick();
            }
        }, SINGLE_CLICK_WAIT);
    }

    touchDown(x, y, id) {
        if (this.active) {
            this.pad.setState(new TouchOneDownTwo(this, x, y, id));
        }
    }
    // touchUp(x, y, id) { }
    // touchMove(x, y, id) { }
}

/**
 * Here we wait for time or movement to start a click drag, if it's released fast enough it's a double click
 * can transition to:
 * - Ready if the touch is released within a given timeframe, send a double click
 * - LeftDrag if moved a given threshold or time passes
 */
class TouchOneDownTwo extends MousepadBase {
    constructor(previous, x, y, id) {
        super(previous);
        this.name = 'TouchOneDownTwo';

        this.x1Start = x;
        this.y1Start = y;
        this.id1 = id;
    }

    activate() {
        super.activate();

        setTimeout(() => {
            if (this.active) {
                this.pad.setState(new LeftDrag(this, this.x1Start, this.y1Start, this.id1));
            }
        }, TOUCH_DRAG_TIME_THRESHOLD);
    }

    // touchDown(x, y, id) { }
    touchUp(x, y, id) {
        if (this.id1 == id && this.active) {
            this.pad.setState(new ReadyState(this));
            this.notifier.leftDoubleClick();
        }
    }
    touchMove(x, y, id) {
        //check for distance moved, if over threshold set to moving state
        var distX = Math.abs(this.x1Start - x);
        var distY = Math.abs(this.y1Start - y);
        if (Math.max(distX, distY) > TOUCH_MOVE_THRESHOLD && this.active) {
            this.pad.setState(new LeftDrag(this, this.x1Start, this.y1Start, id));
        }
    }
}

/**
 * Here all movement is translated to moving, just with an enter and exit condition of left up
 * can transition to:
 * - Ready when the touch is released
 */
class LeftDrag extends MousepadBase {
    constructor(previous, x, y, id) {
        super(previous);
        this.name = 'LeftDrag';
        this.x1Prev = x;
        this.y1Pref = y;
        this.id1 = id;
    }

    activate() {
        super.activate();

        this.notifier.leftDown();
    }

    // touchDown(x, y, id) { }
    touchUp(x, y, id) {
        if (this.active && this.id1 == id) {
            this.pad.setState(new ReadyState(this));
            this.notifier.leftUp();
        }
    }
    touchMove(x, y, id) {
        if (id == this.id1 && this.active) {
            var diffx = this.x1Prev - x;
            var diffy = this.y1Pref - y;
            this.notifier.move(-diffx, -diffy)
            this.x1Prev = x;
            this.y1Pref = y;
        }
    }
}