using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Linq;
using System;
using static UnityEngine.Rendering.DebugUI;

public class FrameBTController : MonoBehaviour
{
    private string DeviceName = "Frame CC";
    private string ServiceUUID = "7A230001-5475-A6A4-654C-8431F6AD49C4";
    private string RXCharacteristic = "7A230003-5475-A6A4-654C-8431F6AD49C4"; // Swapped with TX because this has Notify property
    private string TXCharacteristic = "7A230002-5475-A6A4-654C-8431F6AD49C4"; // Has Write and Write Without Response property

    private bool _workingFoundDevice = true;
    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private bool _foundID = false;
    private int _mtu = 251;
    private string _deviceAddress;

    enum States
    {
        None,
        Scan,
        Connect,
        RequestMTU,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Communication,
    }

    void Reset()
    {
        _workingFoundDevice = false;
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _foundID = false;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    void StartProcess()
    {
        Debug.Log("Initializing...");

        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);
            Debug.Log("Initialized");

        }, (error) => {

            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    // Use this for initialization
    void Start()
    {
        StartProcess();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        Debug.Log("Scanning...");

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {

                            if (name.Contains(DeviceName))
                            {
                                _workingFoundDevice = true;
                                BluetoothLEHardwareInterface.StopScan();
                                Debug.Log("");

                                _deviceAddress = address;

                                Debug.Log("Found Device: " + name + " / " + address);

                                SetState(States.Connect, 0.5f);

                                _workingFoundDevice = false;
                            }

                        }, null, false, false);

                        break;

                    case States.Connect:
                        _foundID = false;

                        Debug.Log("Connecting...");

                        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) => {

                            if (IsEqual(serviceUUID, ServiceUUID))
                            {
                                if (IsEqual(characteristicUUID, RXCharacteristic) || IsEqual(characteristicUUID, TXCharacteristic))
                                {
                                    _connected = true;
                                    SetState(States.RequestMTU, 2f);

                                    Debug.Log("Connected to " + DeviceName + " with address " + _deviceAddress);
                                }
                            }
                        }, (disconnectedAddress) => {
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            Debug.Log("Disconnected from " + DeviceName + " with address " + _deviceAddress);
                        });

                        break;

                    case States.RequestMTU:
                        Debug.Log("Requesting MTU");
                        
                        BluetoothLEHardwareInterface.RequestMtu(_deviceAddress, _mtu, (address, newMTU) =>
                        {
                            Debug.Log("MTU set to " + newMTU.ToString());

                            SetState(States.Subscribe, 0.1f);
                        });                      
                        
                        break;

                    case States.Subscribe:
                        Debug.Log("Subscribing...");

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, ServiceUUID, RXCharacteristic, null, (address, characteristicUUID, bytes) => {
                            // Handle received data here
                            string message = Encoding.UTF8.GetString(bytes);
                            Debug.Log("Received data: " + message);
                        });

                        _state = States.None;
                        Debug.Log("Waiting for data...");
                        
                        break;

                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID, RXCharacteristic, null);
                        SetState(States.Disconnect, 4f);

                        break;

                    case States.Disconnect:
                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) => {
                                BluetoothLEHardwareInterface.DeInitialize(() => {

                                    _connected = false;
                                    _state = States.None;
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => {

                                _state = States.None;
                            });
                        }

                        break;
                }
            }
        }
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }

    public void SendString(string message)
    {
        if (_connected)
        {
            Debug.Log("Sending string: " + message);
            var data = Encoding.UTF8.GetBytes(message);

            BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, ServiceUUID, TXCharacteristic, data, data.Length, false, (characteristicUUID) => {
                Debug.Log("Write Succeeded");
                BluetoothLEHardwareInterface.Log("Write Succeeded");
            });
        }
        else
        {
            Debug.Log("Not connected to any device.");
        }
    }

    // Public method to send messages to Frame
    public void OnButtonPress(string message)
    {
        string luaString = "frame.display.text('" + message + "', 50, 50); frame.display.show()";
        SendString(luaString);
    }

}
