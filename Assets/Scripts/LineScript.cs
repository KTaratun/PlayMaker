using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    static private LineRenderer m_curLine, m_line2;
    static private float m_mouseWheelCurve, bendX, bendY;
    static private List<GameObject> m_pathPool;

    // Start is called before the first frame update
    static public void Start()
    {
        m_mouseWheelCurve = 0;
        bendX = 0;
        bendY = 0;

        m_pathPool = new List<GameObject>();

        for (int i = 0; i < 20; i++)
            CreateLine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public void CreateLine()
    {
        GameObject path = Instantiate(Resources.Load<GameObject>("Path"));

        m_curLine = path.GetComponent<LineRenderer>();
        m_line2 = path.GetComponentsInChildren<LineRenderer>()[1];

        Material blackDiffuseMat = new Material(Shader.Find("Standard"));
        blackDiffuseMat.color = Color.black;
        float width = 0.2f;
        m_curLine.material = blackDiffuseMat;
        m_curLine.startWidth = width;
        m_curLine.endWidth = width;
        m_line2.material = blackDiffuseMat;
        m_line2.startWidth = width;
        m_line2.endWidth = width;

        path.gameObject.SetActive(false);
        m_pathPool.Add(path);
    }

    static public void UpdateLine(Vector2 _mousePos)
    {
        Vector3 newMouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        newMouse.z = 0.0f;

        Vector3 curMouse = new Vector3(_mousePos.x, _mousePos.y, 0);
        Vector3 mainLine = newMouse - curMouse;
        mainLine.Normalize();

        m_curLine.SetPosition(0, _mousePos);
        Vector3 middle = Vector3.Lerp(newMouse, curMouse, .5f / mainLine.magnitude);
        Vector3 other = Vector3.Cross(newMouse - curMouse, Vector3.forward);
        middle += other * m_mouseWheelCurve;
        middle.x += bendX;
        middle.y += bendY;
        m_curLine.SetPosition(1, middle);

        m_line2.SetPosition(0, middle);
        m_line2.SetPosition(1, newMouse);

        SpriteRenderer[] tt = m_curLine.GetComponentsInChildren<SpriteRenderer>();
        Transform sPTrans = m_curLine.GetComponentsInChildren<SpriteRenderer>()[0].transform;
        sPTrans.SetPositionAndRotation(newMouse, Quaternion.identity);

        curMouse.x = middle.x - newMouse.x;
        curMouse.y = middle.y - newMouse.y;
        float angle = Mathf.Atan2(curMouse.y, curMouse.x) * Mathf.Rad2Deg;
        sPTrans.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));

        sPTrans = m_curLine.GetComponentsInChildren<SpriteRenderer>()[2].transform;
        sPTrans.SetPositionAndRotation(middle, Quaternion.identity);

        sPTrans = m_curLine.GetComponentsInChildren<SpriteRenderer>()[3].transform;
        sPTrans.SetPositionAndRotation(_mousePos, Quaternion.identity);

        AdjustColliderToLine(m_curLine, m_curLine.GetPosition(0), m_curLine.GetPosition(1));
        AdjustColliderToLine(m_line2, m_line2.GetPosition(0), m_line2.GetPosition(1));
    }

    static public void ModifyLine(GameObject _point)
    {
        m_curLine = _point.GetComponentInParent<LineRenderer>();
        m_line2 = m_curLine.GetComponentsInChildren<LineRenderer>()[1];

        GameObject beginning = m_curLine.GetComponentsInChildren<SphereCollider>()[2].gameObject;
        GameObject middle = m_curLine.GetComponentsInChildren<SphereCollider>()[1].gameObject;
        GameObject end = m_curLine.GetComponentsInChildren<SphereCollider>()[0].gameObject;

        m_curLine.SetPosition(0, beginning.transform.position);
        m_curLine.SetPosition(1, middle.transform.position);

        m_line2.SetPosition(0, middle.transform.position);
        m_line2.SetPosition(1, end.transform.position);

        Transform sPTrans = m_curLine.GetComponentsInChildren<SpriteRenderer>()[0].transform;
        sPTrans.SetPositionAndRotation(end.transform.position, Quaternion.identity);
        end.transform.SetPositionAndRotation(sPTrans.position, Quaternion.identity);

        Vector3 curMouse = new Vector3();
        curMouse.x = middle.transform.position.x - end.transform.position.x;
        curMouse.y = middle.transform.position.y - end.transform.position.y;
        float angle = Mathf.Atan2(curMouse.y, curMouse.x) * Mathf.Rad2Deg;
        sPTrans.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));

        AdjustColliderToLine(m_curLine, m_curLine.GetPosition(0), m_curLine.GetPosition(1));
        AdjustColliderToLine(m_line2, m_line2.GetPosition(0), m_line2.GetPosition(1));
    }

    static public void BendLine()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // down
            m_mouseWheelCurve += 0.1f;
        else if (Input.GetAxis("Mouse ScrollWheel") > 0) // up
            m_mouseWheelCurve -= 0.1f;

        if (Input.GetKey(KeyCode.W))
            bendY += 0.1f;
        else if (Input.GetKey(KeyCode.S))
            bendY -= 0.1f;

        if (Input.GetKey(KeyCode.A))
            bendX -= 0.1f;
        if (Input.GetKey(KeyCode.D))
            bendX += 0.1f;
    }

    static public void SetFreePath()
    {
        GameObject path = null;
        m_mouseWheelCurve = 0;

        for (int i = 0; i < m_pathPool.Count; i++)
            if (m_pathPool[i].activeSelf == false)
            {
                path = m_pathPool[i];
                m_curLine = path.GetComponent<LineRenderer>();
                m_pathPool.Remove(path);
                m_line2 = m_curLine.GetComponentsInChildren<LineRenderer>()[1];
                m_curLine.gameObject.SetActive(true);
                UndoScript.AddToUndo(m_curLine.gameObject);

                return;
            }

        path = m_curLine.gameObject;
        m_line2 = m_curLine.GetComponentsInChildren<LineRenderer>()[1];
        m_curLine.gameObject.SetActive(true);
        UndoScript.AddToUndo(m_curLine.gameObject);
    }

    static public void FreeUpPath(GameObject _path)
    {
        m_pathPool.Add(_path);
        _path.SetActive(false);
        m_curLine = null;
        m_line2 = null;
    }

    static private void AdjustColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        //create the collider for the line
        BoxCollider lineCollider = line.gameObject.GetComponentInChildren<BoxCollider>();
        // get width of collider from line 
        float lineWidth = line.endWidth;
        // get the length of the line using the Distance method
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        lineCollider.size = new Vector3(lineLength, lineWidth, 1f);
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.position = midPoint;


        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.y - startPoint.y), (endPoint.x - startPoint.x));
        
        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;
        
        //were interested in the inverse so multiply by -1
        //angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    static public void AddToPathPool(GameObject _path) { m_pathPool.Add(_path); }
    static public float GetMouseWheel() { return m_mouseWheelCurve; }
    static public void SetMouseWheel(float _val) { m_mouseWheelCurve = _val; }
    static public GameObject GetCurrentLine() { return m_curLine.gameObject; }
    static public void SetCurrentLine(LineRenderer _curLine) { m_curLine = _curLine; }
    static public void ResetBend() { bendX = 0; bendY = 0; }
}
