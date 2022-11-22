using Newtonsoft.Json;
using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MouseEventArgs = SMT.Utilities.InputEvents.HardwareEvents.MouseEventArgs;

namespace WindowsSocketForms
{
    class MessageHandler
    {
        private readonly MouseEventRunner _mouse;
        private readonly KeyboardEventRunner _keyboard;

        public int MoveScale { get; set; } = 3;
        public int ScrollScale { get; set; } = 3;

        public MessageHandler()
        {
            _mouse = new MouseEventRunner();
            _keyboard = new KeyboardEventRunner();
        }

        public void RunCommand(string msg)
        {
            var cmd = JsonConvert.DeserializeObject<RemoteCommand>(msg);
            Console.WriteLine(cmd.Type);
            switch (cmd.Type)
            {
                case CommandType.mouseMove:
                    var xy = cmd.Data.Split(',');
                    var x = (int)Math.Round(float.Parse(xy[0]));
                    var y = (int)Math.Round(float.Parse(xy[1]));
                    _mouse.DoEvent(MouseEventArgs.Move(x * MoveScale, y * MoveScale));
                    break;
                case CommandType.mouseClick:
                    if (cmd.Data.ToLower() == "left")
                    {
                        _mouse.DoEvent(MouseEventArgs.LeftDown());
                        Thread.Sleep(100);
                        _mouse.DoEvent(MouseEventArgs.LeftUp());
                    }
                    else if (cmd.Data.ToLower() == "right")
                    {
                        _mouse.DoEvent(MouseEventArgs.RightDown());
                        Thread.Sleep(100);
                        _mouse.DoEvent(MouseEventArgs.RightUp());
                    }
                    else if (cmd.Data.ToLower() == "middle")
                    {
                        _mouse.DoEvent(MouseEventArgs.MiddleDown());
                        Thread.Sleep(100);
                        _mouse.DoEvent(MouseEventArgs.MiddleUp());
                    }
                    break;
                case CommandType.mouseDoubleClick:
                    if (cmd.Data.ToLower() == "left")
                    {
                        _mouse.DoEvent(MouseEventArgs.LeftDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.LeftUp());
                        Thread.Sleep(70);
                        _mouse.DoEvent(MouseEventArgs.LeftDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.LeftUp());
                    }
                    else if (cmd.Data.ToLower() == "right")
                    {
                        _mouse.DoEvent(MouseEventArgs.RightDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.RightUp());
                        Thread.Sleep(70);
                        _mouse.DoEvent(MouseEventArgs.RightDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.RightUp());
                    }
                    else if (cmd.Data.ToLower() == "middle")
                    {
                        _mouse.DoEvent(MouseEventArgs.MiddleDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.MiddleUp());
                        Thread.Sleep(70);
                        _mouse.DoEvent(MouseEventArgs.MiddleDown());
                        Thread.Sleep(50);
                        _mouse.DoEvent(MouseEventArgs.MiddleUp());
                    }
                    break;
                case CommandType.mouseDown:
                    if (cmd.Data.ToLower() == "left")
                    {
                        _mouse.DoEvent(MouseEventArgs.LeftDown());
                    }
                    else if (cmd.Data.ToLower() == "right")
                    {
                        _mouse.DoEvent(MouseEventArgs.RightDown());
                    }
                    else if (cmd.Data.ToLower() == "middle")
                    {
                        _mouse.DoEvent(MouseEventArgs.MiddleDown());
                    }
                    break;
                case CommandType.mouseUp:
                    if (cmd.Data.ToLower() == "left")
                    {
                        _mouse.DoEvent(MouseEventArgs.LeftUp());
                    }
                    else if (cmd.Data.ToLower() == "right")
                    {
                        _mouse.DoEvent(MouseEventArgs.RightUp());
                    }
                    else if (cmd.Data.ToLower() == "middle")
                    {
                        _mouse.DoEvent(MouseEventArgs.MiddleUp());
                    }
                    break;
                case CommandType.scrollUp:
                    {
                        var amount = (int)Math.Abs(Math.Round(float.Parse(cmd.Data)));
                        _mouse.DoEvent(MouseEventArgs.Scroll(amount));
                    }
                    break;
                case CommandType.scrollDown:
                    {
                        var amount = -(int)Math.Abs(Math.Round(float.Parse(cmd.Data)));
                        _mouse.DoEvent(MouseEventArgs.Scroll(amount));
                    }
                    break;
                case CommandType.keyboardString:
                    {
                        var keyString = cmd.Data;
                        //TODO come up with a decent definition of virtual and scan keys for emulation
                        //_keyboard.DoEvent(new KeyboardEventArgs(System.Windows.Forms.Keys., 0, true));
                        SendKeys.Send(cmd.Data); //let's see if this works as a decent alternative to actual key emulation
                    }
                    break;
            }
        }
    }

    class RemoteCommand
    {
        /// <summary>
        /// type of command send from the device
        /// </summary>
        public CommandType Type;

        /// <summary>
        /// the button the command is relating to:
        /// left,right,middle relates to mouse commands
        /// any string relates to keyboard commands
        /// move is x,y coords
        /// </summary>
        public string Data;
    }

    enum CommandType
    {
        mouseMove,

        mouseClick,
        mouseDoubleClick,

        mouseDown,
        mouseUp,

        scrollUp,
        scrollDown,

        keyboardString
    }
}
