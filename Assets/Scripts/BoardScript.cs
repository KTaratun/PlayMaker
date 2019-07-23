using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public GameObject m_grid;
    Vector2 m_origPos, m_curMousePos, m_lastMouse;
    GameObject m_object;
    private bool m_lineStart;

    // Start is called before the first frame update
    void Start()
    {
        LineScript.Start();
        UndoScript.Start();
    }

    // Update is called once per frame
    void Update()
    {
        m_lastMouse = m_curMousePos;
        m_curMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        // Make sure the user pressed the mouse down
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Position relative to the eye-point of the camera
            //m_curMousePos -= transform.position;
            m_origPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            // We need to actually hit an object
            RaycastHit hit;
            GameObject gO = null;

            RaycastHit[] rays = Physics.RaycastAll(new Vector3(m_origPos.x, m_origPos.y, 10), -Vector3.forward, 10000);
            for (int i = 0; i < rays.Length; i++)
            {
                if (rays[i].rigidbody)
                {
                    if (rays[i].rigidbody.gameObject.name != "Line" || rays[i].rigidbody.gameObject.name == "Line" && gO == null)
                        gO = rays[i].rigidbody.gameObject;
                }
            }

            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000);
            // BJL: If we use a collider instead of a rigidbody and the distance parameter is too large, we'll get a hit all the time.
            // Could this be due to reflection or because the camera is orthographic, or...?   
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000) && hit.rigidbody && Input.GetMouseButtonDown(0))
            if (Input.GetMouseButtonDown(0) && gO != null)
            {
                m_object = gO;

                if (m_object.name == "Arrow Collider")
                    UndoScript.AddToUndo(LineScript.GetCurrentLine().GetComponentsInChildren<SphereCollider>()[0].gameObject, m_curMousePos);
                else if (m_object.name == "Line")
                    UndoScript.AddToUndo(LineScript.GetCurrentLine());
                else
                    UndoScript.AddToUndo(m_object, m_curMousePos);

                StartCoroutine("DragObject");
            }
            else if (Input.GetMouseButtonDown(1))
            {
                m_lineStart = true;
                LineScript.SetFreePath();
            }
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) &&
            UndoScript.GetUndoPos() > 0)
            UndoScript.Undo(m_object);
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y) &&
            UndoScript.GetUndoObjectsCount() > UndoScript.GetUndoPos())
            UndoScript.Redo(m_object);
        else if (Input.GetKeyDown(KeyCode.Delete))
            DeleteObject();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            m_grid.GetComponent<SpriteRenderer>().enabled = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            m_grid.GetComponent<SpriteRenderer>().enabled = false;

        LineScript.BendLine();

        if (Input.GetMouseButton(1) && m_lineStart)
            LineScript.UpdateLine(m_origPos);
        else if (Input.GetMouseButtonUp(1) && m_lineStart)
        {
            m_lineStart = false;
            LineScript.ResetBend();
            LineScript.SetMouseWheel(0);
        }
    }

    private void DeleteObject()
    {
        GameObject curLine = LineScript.GetCurrentLine();
        curLine.SetActive(false);

        UndoScript.AddToUndo(curLine);
    }

    private IEnumerator DragObject()
    {
        while (Input.GetMouseButton(0))
        {
            // Position on the near clipping plane of the camera in world space
            Vector3 newMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            // Position relative to the eye-point of the camera
            newMousePos -= Camera.main.transform.position;

            if (Input.GetKey(KeyCode.LeftShift) && m_object.layer != LayerMask.NameToLayer("Path"))
            {
                float cellWidth = m_grid.transform.GetComponent<SpriteRenderer>().bounds.size.x / 20.2f;
                float cellHeight = m_grid.transform.GetComponent<SpriteRenderer>().bounds.size.y / 20.2f;

                float newX = Mathf.Round((Mathf.Abs(m_grid.GetComponent<SpriteRenderer>().bounds.min.x) + newMousePos.x) / cellWidth);
                newX = m_grid.GetComponent<SpriteRenderer>().bounds.min.x + 0.05f + (newX * cellWidth);
                float newY = Mathf.Round((Mathf.Abs(m_grid.GetComponent<SpriteRenderer>().bounds.min.y) + newMousePos.y) / cellHeight);
                newY = m_grid.GetComponent<SpriteRenderer>().bounds.min.y + (newY * cellHeight);

                m_object.transform.position = new Vector3(newX, newY, 0);
                m_curMousePos = new Vector2(newX, newY);
            }
            else
            {
                if (m_object.name == "Arrow Collider")
                    m_object = LineScript.GetCurrentLine().GetComponentsInChildren<SphereCollider>()[0].gameObject;
                else if (m_object.name == "Line")
                    m_object = LineScript.GetCurrentLine();

                float diffX = m_lastMouse.x - m_curMousePos.x;
                float diffY = m_lastMouse.y - m_curMousePos.y;
                Debug.Log(diffX + "\n" + diffY);
                //Vector3 oldPos = m_object.transform.position;
                //m_object.transform.position = new Vector3(m_curMousePos.x, m_curMousePos.y, 0);
                m_object.transform.SetPositionAndRotation(new Vector3(m_object.transform.position.x - diffX, m_object.transform.position.y - diffY, m_object.transform.position.z), Quaternion.identity);
                //Transform g = GameObject.Find("Guard L").transform;
                //float xAdded = oldPos.x - m_object.transform.position.x;
                //g.SetPositionAndRotation(new Vector3(g.position.x - xAdded, g.position.y - (oldPos.y - m_object.transform.position.y)), Quaternion.identity);
                m_curMousePos = newMousePos;

                if (m_object.layer == LayerMask.NameToLayer("Path"))
                    LineScript.ModifyLine(m_object);
                else
                    LineScript.SetCurrentLine(null);
            }

            yield return 0;
        }

        m_object = null;
    }
}
