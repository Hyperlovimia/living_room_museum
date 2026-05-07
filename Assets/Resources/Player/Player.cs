using System;
using System.Linq.Expressions;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

/// <summary>
/// 玩家
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// 角色控制器
    /// </summary>
    private CharacterController controller;

    /// <summary>
    /// 玩家移动速度
    /// </summary>
    public float speed = 5f;

    /// <summary>
    /// 重力
    /// </summary>
    public float gravity = -15f;

    /// <summary>
    /// 速度方向
    /// </summary>
    Vector3 velocity;

    public static Player _;

    private MouseLook _mouseLook;
    private bool _isPause;


    private void Awake()
    {
        _ = this;
        controller = transform.GetComponent<CharacterController>();
        _mouseLook = transform.GetComponentInChildren<MouseLook>();
        Pause();
    }

    private float _timer;
    // Update is called once per frame

    public void Pause()
    {
        _isPause = true;
        _mouseLook.IsPause = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReStart()
    {
        _isPause = false;
        _mouseLook.IsPause = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (_isPause) return;
        //获取玩家键盘的输入 wsad
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //玩家移动方向
        Vector3 move = transform.right * x + transform.forward * z;

        //玩家进行移动
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && _timer <= 0)
        {
            velocity.y = 5;
            _timer = 1;
        }

        if (_timer > 0)
            _timer -= Time.deltaTime;
        if (_timer < 0)
            _timer = 0;
        //重力
        velocity.y += gravity * Time.deltaTime;
        //重力方向移动
        controller.Move(velocity * Time.deltaTime);


        //射击
        if (Input.GetMouseButtonDown(0) && !_isPause)
        {
            Debug.Log("shut");
            //从屏幕中心方向发射一条射线
            var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            //射线如果碰撞到物体
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.TryGetComponent(out DoBase comp))
            {
                //comp.Do();
                Debug.Log(hit.collider.gameObject.name);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_HOME);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "boxEnter")
            ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_CHAT);
    }
}