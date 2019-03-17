package com.potatosoft.remote.phoneremote;

/**
 * Created by seanm_000 on 9/17/2017.
 */

public class MessageBuilder {

    private static int INVALID_COMMAND = 0;
    private static int MOUSE_POSITION_MOVE = 1;
    private static int MOUSE_POSITION_SET = 2;
    private static int MOUSE_LEFT_CLICK = 10;
    private static int MOUSE_LEFT_DOUBLE_CLICK = 11;
    private static int MOUSE_LEFT_PRESS = 12;
    private static int MOUSE_LEFT_RELEASE = 13;

    private static int MOUSE_RIGHT_CLICK = 20;
    private static int MOUSE_RIGHT_DOUBLE_CLICK = 21;
    private static int MOUSE_RIGHT_PRESS = 22;
    private static int MOUSE_RIGHT_RELEASE = 23;

    private static int MOUSE_SCROLLWHEEL_CLICK = 30;
    private static int MOUSE_SCROLLWHEEL_SCROLL_MOVE = 31;

    private static int KEYBOARD_KEY_CLICK = 40;
    private static int KEYBOARD_KEY_PRESS = 41;
    private static int KEYBOARD_KEY_RELEASE = 42;
    private static int KEYBOARD_KEY_STRING = 43;

    public static class Keys {
        public static String MEDIA_NEXT = "media-next";
        public static String MEDIA_PLAY_PAUSE = "media-play-pause";
    }

    public static String MouseMove(int x, int y) {
        return "[" + MOUSE_POSITION_MOVE + ":" + x + "," + y + ":]";
    }

    public static String LeftClick() {
        return "[" + MOUSE_LEFT_CLICK + "::]";
    }

    public static String LeftDoubleClick() {
        return "[" + MOUSE_LEFT_DOUBLE_CLICK + "::]";
    }

    public static String LeftPress() {
        return "[" + MOUSE_LEFT_PRESS + "::]";
    }

    public static String LeftRelease() {
        return "[" + MOUSE_LEFT_RELEASE + "::]";
    }

    public static String RightClick() {
        return "[" + MOUSE_RIGHT_CLICK + "::]";
    }

    public static String Scrollwheel(int amount) {
        return "[" + MOUSE_SCROLLWHEEL_SCROLL_MOVE + ":" + amount + ":]";
    }

    public static String KeyboardKeyClick(String keyName) {
        return "[" + KEYBOARD_KEY_CLICK + ":" + keyName + ":]";
    }
}
