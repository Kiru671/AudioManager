using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class Notification : MonoBehaviour
{
    public bool permission;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        RequestAndroidPermission();
        InitNotificationGroups();
#endif

#if UNITY_IOS
        RequestIosPermission();
#endif
    }

#if UNITY_ANDROID
    public void InitNotificationGroups()
    {
        var group = new AndroidNotificationChannelGroup()
        {
            Id = "Main",
            Name = "Test Noitifications"
        };
        AndroidNotificationCenter.RegisterNotificationChannelGroup(group);

        var channel = new AndroidNotificationChannel()
        {
            Id = "test_ch",
            Name = "Test Channel",
            Importance = Importance.Default,
            Description = "For testing only",
            Group = "Main"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        Debug.Log("Created Groups");
    }

    public void RequestAndroidPermission()
    {
        StartCoroutine(RequestNotificationPermission());
    }

    IEnumerator RequestNotificationPermission()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;
        if (request.Status == PermissionStatus.Allowed)
            permission = true;

        AndroidNotificationCenter.RequestExactScheduling();

        Debug.Log("Requested Permission");
    }

    public void SendAndroidNotification()
    {
        var notification = new AndroidNotification();
        notification.Title = "Test Successful";
        notification.Text = "Notifications are working!";
        notification.FireTime = System.DateTime.Now.AddSeconds(30);

        AndroidNotificationCenter.SendNotification(notification, "test_ch");
    }
#endif

#if UNITY_IOS
    public void RequestIosPermission()
    {
        StartCoroutine(RequestNotificationPermission());
    }

    IEnumerator RequestNotificationPermission()
    {
        using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, false))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            permission = req.Granted;
        }
    }

    public void SendIosNotification()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new System.TimeSpan(0, 0, 30),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "test_01",
            Title = "Test Successful",
            Body = "Notifications are working!",
            Subtitle = "What is this ?",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Badge),
            CategoryIdentifier = "TestCategory",
            ThreadIdentifier = "TestThread",
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
#endif


    public void ButtonPressed()
    {

#if UNITY_ANDROID
        SendAndroidNotification();
#endif

#if UNITY_IOS
            SendIosNotification();
#endif

    }



}
