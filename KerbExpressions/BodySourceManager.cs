using UnityEngine;
using System.Collections;
using System.Linq;
using Windows.Kinect;
using KerbExpressions;

public class BodySourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _bodyArray = null;

    private int _bodyCount = 0;

    public Body[] Bodies
    {
        get
        {
            return _bodyArray;
        }
    }

    public Body PrimaryBody { get; private set; }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        Util.Log("Started BodySourceManager. Found Sensor: {0}", (_Sensor != null));

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {

        bool frameUpdated = false;

        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_bodyArray == null)
                {
                    _bodyArray = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_bodyArray);
                frameUpdated = true;

                frame.Dispose();
                frame = null;
            }
        }

        if (frameUpdated)
        {
            ProcessData();
        }
    }

    private void ProcessData()
    {
        var trackedBodies = _bodyArray.Where(b => b.IsTracked);

        int newBodyCount = trackedBodies.Count();

        if (_bodyCount != newBodyCount)
        {
            Util.Log("Now see {0} bodies", newBodyCount);
        }

        _bodyCount = newBodyCount;

        SelectPrimaryBody();
    }

    private void SelectPrimaryBody()
    {
        PrimaryBody = _bodyArray.Where(b => b.IsTracked && b.Joints[JointType.SpineMid].TrackingState != TrackingState.NotTracked)
                                .OrderBy(b => b.Joints[JointType.SpineMid].Position.Z * Mathf.Abs(b.Joints[JointType.SpineMid].Position.X))
                                .FirstOrDefault();
    }

    void OnApplicationQuit()
    {
        Util.Log("Cleaning up Kinect...");
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
