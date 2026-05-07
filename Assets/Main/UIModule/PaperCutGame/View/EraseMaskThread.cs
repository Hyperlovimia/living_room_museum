
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = System.Object;

[RequireComponent(typeof(RawImage))]
public class EraseMaskThread : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    /// <summary>
    /// 是否擦除了
    /// </summary>
    public bool isStartEraser;

    /// <summary>
    /// 是否擦除结束了
    /// </summary>
    public bool isEndEraser;

    /// <summary>
    /// 开始事件
    /// </summary>
    public Action eraserStartEvent;

    /// <summary>
    /// 结束事件
    /// </summary>
    public Action eraserEndEvent;

    public RawImage uiTex;
    Texture2D tex;

    /// <summary>
    /// 监听事件
    /// </summary>
    /// <param name="eraseComplete">完成事件</param>
    /// <param name="eraseStart">开始事件</param>
    public void RegisterNotify(Action eraseComplete, Action eraseStart = null)
    {
        this.eraserEndEvent = eraseComplete;
        this.eraserStartEvent = eraseStart;
    }

    /// <summary>
    /// 是否在自动滑动
    /// </summary>
    public bool IsAuto = false;

    bool FirstAuto = true;

    /// <summary>
    /// 自动擦除
    /// </summary>
    /// <param name="v">擦除坐标</param>
    public void AutoErase(Vector2 pos)
    {
        if (!IsAuto)
        {
            return;
        }

        if (FirstAuto)
        {
            penultPos = pos;
            CheckPoint(pos);
            FirstAuto = false;
            return;
        }

        if (twoPoints && Vector2.Distance(pos, lastPos) > distance) //如果两次记录的鼠标坐标距离大于一定的距离，开始记录鼠标的点
        {
            float dis = Vector2.Distance(lastPos, pos);
            CheckPoint(pos);
            int segments = (int) (dis / radius); //计算出平滑的段数                                              
            segments = segments < 1 ? 1 : segments;
            if (segments >= 10)
            {
                segments = 10;
            }

            pointsOnDrag = Beizier(penultPos, lastPos, pos, segments); //进行贝塞尔平滑
            for (int i = 0; i < pointsOnDrag.Length; i++)
            {
                CheckPoint(pointsOnDrag[i]);
            }

            lastPos = pos;
            if (pointsOnDrag.Length > 2)
                penultPos = pointsOnDrag[pointsOnDrag.Length - 2];
        }
        else
        {
            twoPoints = true;
            lastPos = pos;
        }
    }


    Texture2D MyTex;
    int mWidth;
    int mHeight;

    /// <summary>
    /// 刷子尺寸
    /// </summary>
    [Header("Brush Size")] public int brushSize = 50;

    /// <summary>
    /// 刷子不是刷消失，反而是刷出现
    /// </summary>
    [Header("Brush Appear")] public bool isBrushAppear = false;

    /// <summary>
    /// 完成的擦除比列
    /// </summary>
    [Header("Rate")] public int rate = 90;

    float maxColorA;
    float colorA;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void Awake()
    {
   Init();
    }

    public void Init()
    {
        Initial();
        if (isBrushAppear)
        {
            checkColorStore2 = new Color[mWidth * mHeight];
            Color[] color = MyTex.GetPixels();
            //checkColorStore = new Texture2D(mWidth,mHeight);
            //checkColorStore.SetPixels(color);
            for (int i = 0; i < color.Length; i++)
            {
                checkColorStore2[i] = color[i];
                color[i] = new Color(color[i].r, color[i].g, color[i].b, 0);
            }

            checkColorNow = color;
            MyTex.SetPixels(color);
            MyTex.Apply();
        }
        else
        {
            checkColorStore2 = MyTex.GetPixels();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initial()
    {
        tex = (Texture2D) uiTex.mainTexture;
        MyTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        mWidth = MyTex.width;
        mHeight = MyTex.height;

        MyTex.SetPixels(tex.GetPixels());
        MyTex.Apply();
        uiTex.texture = MyTex;
        maxColorA = MyTex.GetPixels().Length;
        colorA = 0;
        isEndEraser = false;
        isStartEraser = false;
    }

    //private void OnDisable()
    //{
    //    MyTex = null;
    //    tex = null;
    //}

    /// <summary>
    /// 贝塞尔平滑
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="mid">中点</param>
    /// <param name="end">终点</param>
    /// <param name="segments">段数</param>
    /// <returns></returns>
    public Vector2[] Beizier(Vector2 start, Vector2 mid, Vector2 end, int segments)
    {
        float d = 1f / segments;
        Vector2[] points = new Vector2[segments - 1];
        for (int i = 0; i < points.Length; i++)
        {
            float t = d * (i + 1);
            points[i] = (1 - t) * (1 - t) * mid + 2 * t * (1 - t) * start + t * t * end;
        }

        List<Vector2> rps = new List<Vector2>();
        rps.Add(mid);
        rps.AddRange(points);
        rps.Add(end);
        return rps.ToArray();
    }


    bool startDraw = false;
    bool twoPoints = false;
    Vector2 lastPos; //最后一个点
    Vector2 penultPos; //倒数第二个点
    float radius = 12f;
    float distance = 1f;


    #region 事件

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEndEraser)
        {
            return;
        }

        IsAuto = false;
        CheckPoint(penultPos);
        startDraw = true;
        twoPoints = false;
        penultPos = eventData.position;
    }

    Vector2[] pointsOnDrag;

    public void OnDrag(PointerEventData eventData)
    {
        if (isEndEraser)
        {
            return;
        }

        IsAuto = false;
        if (twoPoints && Vector2.Distance(eventData.position, lastPos) > distance) //如果两次记录的鼠标坐标距离大于一定的距离，开始记录鼠标的点
        {
            Vector2 pos = eventData.position;
            float dis = Vector2.Distance(lastPos, pos);
            CheckPoint(eventData.position);
            int segments = (int) (dis / radius); //计算出平滑的段数                                              
            segments = segments < 1 ? 1 : segments;
            if (segments >= 10)
            {
                segments = 10;
            }

            pointsOnDrag = Beizier(penultPos, lastPos, pos, segments); //进行贝塞尔平滑
            for (int i = 0; i < pointsOnDrag.Length; i++)
            {
                CheckPoint(pointsOnDrag[i]);
            }

            lastPos = pos;
            if (pointsOnDrag.Length > 2)
                penultPos = pointsOnDrag[pointsOnDrag.Length - 2];
        }
        else
        {
            twoPoints = true;
            lastPos = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isEndEraser)
        {
            return;
        }

        //CheckPoint(eventData.position);
        startDraw = false;
        twoPoints = false;
    }

    #endregion

    Canvas canvas;

    Color[] checkColorStore2; //储存MyTex.GetPixels();（IsBruchAppear == true值一直不变），（IsBruchAppear == false值会改变）
    Color[] checkColorNow; //储存当前值，以免每次都读取图像
    Color[] checkColorSubmit; //最终需要提交上去的值

    Color[] checkColorSubmitForSmooth; //最终需要提交上去的值，但是是为了平滑

    Vector3 checkPointWorldPos;
    Vector3 checkPointLocalPos;

    Vector2 brushWH; //长宽
    Vector2 pressLoc; //在屏幕中的绘制起点位置
    Vector2 pressLocBase; //在面板中绘制的原本的中心位置

    Vector2 pressLocOld; //在屏幕中的绘制起点位置

    private bool dragLock = false;
    private bool updateCheckPoint = false;
    private bool destroyThis = false;
    private Thread checkPointThread = null;

    /// <summary>
    /// 用于临时存储数据的
    /// </summary>
    private float temp1;

    void CheckPoint(Vector3 pScreenPos)
    {
        try
        {
            if (dragLock)
            {
                return;
            }

            dragLock = true;
            checkPointWorldPos = canvas.worldCamera.ScreenToWorldPoint(pScreenPos);
            checkPointLocalPos = uiTex.gameObject.transform.InverseTransformPoint(checkPointWorldPos);
            checkPointLocalPos = new Vector3((int) checkPointLocalPos.x, (int) checkPointLocalPos.y);

            if (checkPointLocalPos.x > -mWidth / 2 && checkPointLocalPos.x < mWidth / 2 &&
                checkPointLocalPos.y > -mHeight / 2 && checkPointLocalPos.y < mHeight / 2)
            {
                //先计算指块的大小
                brushWH = new Vector2(2 * brushSize, 2 * brushSize); //长宽
                pressLoc = new Vector2(checkPointLocalPos.x - brushSize,
                    checkPointLocalPos.y - brushSize); //在屏幕中的绘制起点位置
                pressLocBase = new Vector2(brushSize, brushSize); //在面板中绘制的原本的中心位置
                if (checkPointLocalPos.x - brushSize < -mWidth / 2)
                {
                    brushWH.x = mWidth / 2 + checkPointLocalPos.x + brushSize;
                    pressLoc.x = -mWidth / 2;
                    pressLocBase.x = mWidth / 2 + checkPointLocalPos.x;
                }
                else if (checkPointLocalPos.x + brushSize > mWidth / 2)
                {
                    brushWH.x = mWidth / 2 - checkPointLocalPos.x + brushSize;
                }

                if (checkPointLocalPos.y - brushSize < -mHeight / 2)
                {
                    brushWH.y = mHeight / 2 + checkPointLocalPos.y + brushSize;
                    pressLoc.y = -mHeight / 2;
                    pressLocBase.y = mHeight / 2 + checkPointLocalPos.y;
                }
                else if (checkPointLocalPos.y + brushSize > mHeight / 2)
                {
                    brushWH.y = mHeight / 2 - checkPointLocalPos.y + brushSize;
                }

                //checkColorStore = new Color[(int)(brushWH.x * brushWH.y)];
                checkColorSubmit = MyTex.GetPixels((int) pressLoc.x + (int) mWidth / 2,
                    (int) mHeight / 2 + (int) pressLoc.y, (int) brushWH.x, (int) brushWH.y);
                updateCheckPoint = true;

                if (checkPointThread == null)
                {
                    checkPointThread = new Thread(() =>
                    {
                        while (!destroyThis)
                        {
                            if (updateCheckPoint)
                            {
                                updateCheckPoint = false;
                                if (isBrushAppear)
                                {
                                    for (int j = (int) brushWH.y - 1; j >= 0; j--)
                                    {
                                        for (int i = 0; i < (int) brushWH.x; i++)
                                        {
                                            //中间取目标，边缘取
                                            if ((i - pressLocBase.x) * (i - pressLocBase.x) +
                                                (j - pressLocBase.y) * (j - pressLocBase.y) > brushSize * brushSize)
                                            {
                                                //边缘，取0，如果已经有值，则取已经的指
                                                //checkColorStore[(int)(i + j * brushWH.x)] = checkColorNow[(int)((int)pressLoc.x + (int)mWidth / 2 + i + ((int)mHeight / 2 + (int)pressLoc.y  + j) * mWidth)];
                                            }
                                            else
                                            {
                                                //中间取目标
                                                //Debug.Log(checkColorStore2.Length+"="+(int)(pressLoc.x + i + (pressLoc.y + j) * mWidth));
                                                temp1 = checkColorNow[
                                                                (int) ((int) pressLoc.x + (int) mWidth / 2 + i +
                                                                       ((int) mHeight / 2 + (int) pressLoc.y + j) *
                                                                       mWidth)]
                                                            .a -
                                                        checkColorStore2[
                                                                (int) ((int) pressLoc.x + (int) mWidth / 2 + i +
                                                                       ((int) mHeight / 2 + (int) pressLoc.y + j) *
                                                                       mWidth)]
                                                            .a;
                                                temp1 = temp1 > 0 ? temp1 : -temp1;
                                                if (temp1 >= 0.01f)
                                                {
                                                    colorA++;
                                                    checkColorNow[
                                                                (int) ((int) pressLoc.x + (int) mWidth / 2 + i +
                                                                       ((int) mHeight / 2 + (int) pressLoc.y + j) *
                                                                       mWidth)]
                                                            .a
                                                        = checkColorStore2[
                                                                (int) ((int) pressLoc.x + (int) mWidth / 2 + i +
                                                                       ((int) mHeight / 2 + (int) pressLoc.y + j) *
                                                                       mWidth)]
                                                            .a;
                                                    checkColorSubmit[(int) (i + j * brushWH.x)] =
                                                        checkColorStore2[
                                                            (int) ((int) pressLoc.x + (int) mWidth / 2 + i +
                                                                   ((int) mHeight / 2 + (int) pressLoc.y + j) *
                                                                   mWidth)];
                                                }
                                                else
                                                {
                                                    //checkColorStore[(int) (i + j * brushWH.x)] = checkColorNow[ (int) ((int) pressLoc.x + (int) mWidth / 2 + i + ((int) mHeight / 2 + (int) pressLoc.y + j) * mWidth)];
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int j = (int) brushWH.y - 1; j >= 0; j--)
                                    {
                                        for (int i = 0; i < (int) brushWH.x; i++)
                                        {
                                            //中间取0，边缘取原本
                                            if (Mathf.Pow(i - pressLocBase.x, 2) + Mathf.Pow(j - pressLocBase.y, 2) >
                                                Mathf.Pow(brushSize, 2))
                                            {
                                                //边缘
                                                //checkColorStore[(int) (i + j * brushWH.x)] = checkColorStore2[(int) ((int)pressLoc.x + (int)mWidth / 2 + i + ((int)mHeight / 2 + (int)pressLoc.y + j) * mWidth)];
                                            }
                                            else
                                            {
                                                //中间
                                                if (checkColorStore2[
                                                        (int) pressLoc.x + (int) mWidth / 2 + i +
                                                        ((int) mHeight / 2 + (int) pressLoc.y + j) * mWidth].a != 0f)
                                                {
                                                    colorA++;
                                                    checkColorStore2[
                                                            (int) pressLoc.x + (int) mWidth / 2 + i +
                                                            ((int) mHeight / 2 + (int) pressLoc.y + j) * mWidth] =
                                                        Color.clear;
                                                    checkColorSubmit[(int) (i + j * brushWH.x)] = Color.clear;
                                                }
                                            }
                                        }
                                    }
                                }

                                /*//检测是否是每次按下的第一次按下，如果不是第一次，且距离超越了一个半径，则连接两次的中间空间
                                //判断checkColorNow和checkColorStore2的差距，差距小就return
                                if (startDraw && getDistance(pressLocOld, pressLoc) > brushSize)
                                {
                                    //计算两次按下的距离，算出切点，再算出长方形checkColorSubmitForSmooth = MyTex.GetPixels的部分
                                    
                                    //遍历算出长方形中所处的长方形
                                    //再整个擦除图外的执行continue
                                    if (isBrushAppear) {
                                        //判断checkColorNow和checkColorStore2的差距，差距大就colorA++;并且修改checkColorNow和checkColorSubmitForSmooth
                                    } else {
                                        //判断checkColorStore2是否为0，为0就colorA++;并且修改checkColorStore2和checkColorSubmitForSmooth为Color.clear
                                    }
                                    MainThreadDispatcher.Send(_ =>
                                    {
                                        MyTex.SetPixels((int)pressLoc.x + (int)mWidth / 2, (int)mHeight / 2 + (int)pressLoc.y,  (int)brushWH.x,(int)brushWH.y,checkColorSubmit);
                                        MyTex.SetPixels((int)pressLoc.x + (int)mWidth / 2, (int)mHeight / 2 + (int)pressLoc.y,  (int)brushWH.x,(int)brushWH.y,checkColorSubmitForSmooth);
                                        MyTex.Apply();
    
                                        dragLock = false;
                                    },0);
                                }
                                else
                                {
                                    MainThreadDispatcher.Send(_ =>
                                    {
                                        MyTex.SetPixels((int)pressLoc.x + (int)mWidth / 2, (int)mHeight / 2 + (int)pressLoc.y,  (int)brushWH.x,(int)brushWH.y,checkColorSubmit);
                                        MyTex.Apply();
    
                                        dragLock = false;
                                    },0);
                                }
    
                                pressLocOld = pressLoc;*/

                                MainThreadDispatcher.Send(_ =>
                                {
                                    MyTex.SetPixels((int) pressLoc.x + (int) mWidth / 2,
                                        (int) mHeight / 2 + (int) pressLoc.y, (int) brushWH.x, (int) brushWH.y,
                                        checkColorSubmit);
                                    MyTex.Apply();

                                    dragLock = false;
                                }, 0);
                            }
                        }
                    });
                    checkPointThread.Start();
                }

                //开始刮的时候 去判断进度
                if (!isStartEraser && !IsAuto)
                {
                    isStartEraser = true;
                    InvokeRepeating("getTransparentPercent", 0f, 0.2f);
                    if (eraserStartEvent != null)
                    {
                        eraserStartEvent.Invoke();
                    }
                }
            }
            else
            {
                dragLock = false;
            }
        }
        catch (Exception e)
        {
        }
    }

    private float getDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    public void destroyThisComponent()
    {
        destroyThis = true;
    }

    public void OnDestroy()
    {
        destroyThis = true;
    }

    double fate;


    public void runToEnd()
    {
        if (isBrushAppear)
        {
            MyTex.SetPixels(0, 0, mWidth, mHeight, checkColorStore2);
            MyTex.Apply();
        }
    }

    public float getTheProgress() //获取涂抹进度，0-1
    {
        return (float) fate / rate;
    }

    /// <summary> 
    /// 检测当前刮刮卡 进度
    /// </summary>
    /// <returns></returns>
    public void getTransparentPercent()
    {
        //Debug.Log("是否限制："+dragLock);
        if (isEndEraser)
        {
            return;
        }


        fate = colorA / maxColorA * 100;

        fate = (float) Math.Round(fate, 2);

        //Debug.LogError("当前百分比: " + fate);

        if (fate >= rate)
        {
            isEndEraser = true;
            CancelInvoke("getTransparentPercent");
            // gameObject.SetActive(false);
            Debug.Log("擦除完毕");
            //触发结束事件
            if (eraserEndEvent != null)
            {
                eraserEndEvent.Invoke();
            }
        }
    }
}