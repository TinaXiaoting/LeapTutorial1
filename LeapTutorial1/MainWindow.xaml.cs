using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

using Leap;


namespace LeapTutorial1
{

    //Main windows that set up and start the project
    public partial class MainWindow : Window, ILeapEventDelegate
    {
        private Controller controller = new Controller();
        private LeapEventListener listener;

        public MainWindow()
        {
            InitializeComponent();
            //Create controller object in Main window
            this.controller = new Controller();
            //Create Listener
            this.listener = new LeapEventListener(this);
            controller.AddListener(listener);
            
        }

        delegate void LeapEventDelegate(string EventName);


        //This method check the event in listener class
        //The activated event's name can be get through this method
        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        Debug.WriteLine("Init");
                        break;
                    case "onConnect":
                        this.connectHandler();
                        break;
                    case "onFrame": 
                        Debug.WriteLine("On Frame");
                        this.checkGestures(this.controller.Frame());
                        this.newFrameHandler(this.controller.Frame());
                        this.CountNumber(this.controller.Frame());

                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        public void connectHandler()

        {
            this.controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_IMAGES);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            this.controller.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);
        }
        public void CountNumber(Leap.Frame frame)
        {
           //create handlist
            HandList hands = frame.Hands;

            //get the front most hand
            Hand frontmost = hands.Frontmost;

            //pass the front most hand type to label
            if (frontmost.IsLeft)
            {
                frontmostType.Content = "Left";
            }
            else 
            {
                frontmostType.Content = "Right";
            }

            //pass the hand's basic information to its labels
            foreach (Hand hand in hands)
            {

                int handCount = hands.Count();
                Leap.Vector direction = hand.Direction;
                Leap.Vector PalmNormal = hand.PalmNormal;
                Leap.Vector center = hand.PalmPosition;
                Leap.Vector moveRate = hand.PalmVelocity;

                HandCount.Content = handCount;
                Direction.Content = direction;
                PalmPosition.Content = center;
                normalVector.Content = PalmNormal;

                //pass finger number in front hand to label( it is always five, with the new Leap Motion Hand model)
                FingerList fingers = hand.Fingers;
                FingerCount.Content = fingers.Count();

            }
        }

        public void checkGestures(Leap.Frame frame)
        {
            //create a gesture list 
            GestureList gestures = frame.Gestures();

            //access every getures that is detected
            foreach (Gesture gesture in gestures)
            {
               // Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPE_CIRCLE:
                        CircleGesture circle = new CircleGesture(gesture);

                        // Calculate clock direction using the angle between circle normal and pointable
                        String clockwiseness;
                        if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 2)
                        {
                            //Clockwise if angle is less than 90 degrees
                            clockwiseness = "clockwise";
                        }
                        else
                        {
                            clockwiseness = "counterclockwise";
                        }

                        float sweptAngle = 0;

                        // Calculate angle swept since last frame
                        if (circle.State != Gesture.GestureState.STATE_START)
                        {
                            CircleGesture previousUpdate = new CircleGesture(controller.Frame(1).Gesture(circle.Id));
                            sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
                        }

                        //fill things into labels: id, state, progress, radius, angle
                        circleLB.Content = "  Circle id: " + circle.Id
                                       + ", " + circle.State
                                       + ", progress: " + circle.Progress
                                       + ", radius: " + circle.Radius
                                       + ", angle: " + sweptAngle
                                       + ", " + clockwiseness;

                       Debug.WriteLine("  Circle id: " + circle.Id
                                       + ", " + circle.State
                                       + ", progress: " + circle.Progress
                                       + ", radius: " + circle.Radius
                                       + ", angle: " + sweptAngle
                                       + ", " + clockwiseness);
                        break;
                    case Gesture.GestureType.TYPE_SWIPE:
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        swipeLB.Content = "  Swipe id: " + swipe.Id
                                       + ", " + swipe.State
                                       + ", position: " + swipe.Position
                                       + ", direction: " + swipe.Direction
                                       + ", speed: " + swipe.Speed;

                        Debug.WriteLine("  Swipe id: " + swipe.Id
                                       + ", " + swipe.State
                                       + ", position: " + swipe.Position
                                       + ", direction: " + swipe.Direction
                                       + ", speed: " + swipe.Speed);
                        break;
                    case Gesture.GestureType.TYPE_KEY_TAP:
                        KeyTapGesture keytap = new KeyTapGesture(gesture);
                        keyTapLB.Content = "  Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction;

                        Debug.WriteLine("  Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction);
                        break;
                    case Gesture.GestureType.TYPE_SCREEN_TAP:
                        ScreenTapGesture screentap = new ScreenTapGesture(gesture);
                        screenTapLB.Content = "  Tap id: " + screentap.Id
                                       + ", " + screentap.State
                                       + ", position: " + screentap.Position
                                       + ", direction: " + screentap.Direction;

                        Debug.WriteLine("  Tap id: " + screentap.Id
                                       + ", " + screentap.State
                                       + ", position: " + screentap.Position
                                       + ", direction: " + screentap.Direction);
                        break;
                    default:
                        Debug.WriteLine("  Unknown gesture type.");
                        break;
                }
            }
           
        }
         void newFrameHandler(Leap.Frame frame)
        {
            this.ID.Content = frame.Id.ToString();
            this.FrameRate.Content = frame.CurrentFramesPerSecond.ToString();
            this.IsValid.Content = frame.IsValid.ToString();
          
        }

        void MainWindow_Closing(object sender, EventArgs e)
        {
            this.controller.RemoveListener(this.listener);
            this.controller.Dispose();
        }
    }


    public interface ILeapEventDelegate
    {
        //definded a method that can be reused
        void LeapEventNotification(string EventName);
    }

    //listener class
    public class LeapEventListener : Listener
    {
      
        //create a interface 
        ILeapEventDelegate eventDelegate;

        //create a constructor
        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
            
        }

        public override void OnInit(Controller controller)
        {
            /**call the LeapEventNotification method in the eventDelegate interface. 
           If the event is activated, the event name can be reported to LeapEventNotification 
             */
            this.eventDelegate.LeapEventNotification("onInit");
        }
        public override void OnConnect(Controller controller)
        {
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_IMAGES);
            //enable all four types of gestures
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.eventDelegate.LeapEventNotification("onConnect");
        }

        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }

    }

}
