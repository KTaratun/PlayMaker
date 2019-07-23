using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoScript : MonoBehaviour
{
    static List<UndoItem> m_undoObjects;
    static private int m_undoPos;

    class UndoItem
    {
        GameObject obj;
        Vector2 pos;

        public UndoItem(GameObject _obj)
        {
            obj = _obj;
            pos = new Vector2(0, 0);
        }
        public UndoItem(GameObject _obj, Vector2 _pos)
        {
            obj = _obj;
            pos = _pos;
        }
        public GameObject GetObj() { return obj; }
        public Vector2 GetPos() { return pos; }
        public void SetPos(Vector2 _pos) { pos = _pos; }
    }

    // Start is called before the first frame update
    static public void Start()
    {
        m_undoObjects = new List<UndoItem>();
        m_undoPos = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public void Undo(GameObject _gO)
    {
        UndoItem uItem = m_undoObjects[m_undoPos - 1];
        _gO = uItem.GetObj();
        m_undoPos--;

        if (_gO.GetComponent<Rigidbody>() == null)
        {
            if (_gO.activeSelf)
                _gO.SetActive(false);
            else
                _gO.SetActive(true);

            return;
        }

        Vector2 temp = _gO.transform.position;
        _gO.transform.SetPositionAndRotation(new Vector3(uItem.GetPos().x, uItem.GetPos().y, 0), Quaternion.identity);
        uItem.SetPos(temp);

        if (_gO.layer == LayerMask.NameToLayer("Path"))
            LineScript.ModifyLine(_gO);
    }

    static public void Redo(GameObject _gO)
    {
        m_undoPos++;
        UndoItem uItem = m_undoObjects[m_undoPos - 1];
        _gO = uItem.GetObj();

        if (_gO.GetComponent<Rigidbody>() == null)
        {
            if (_gO.activeSelf)
                _gO.SetActive(false);
            else
                _gO.SetActive(true);

            return;
        }

        Vector2 temp = _gO.transform.position;
        _gO.transform.SetPositionAndRotation(new Vector3(uItem.GetPos().x, uItem.GetPos().y, 0), Quaternion.identity);
        uItem.SetPos(temp);

        if (_gO.layer == LayerMask.NameToLayer("Path"))
            LineScript.ModifyLine(_gO);
    }

    static public void AddToUndo(GameObject _gO)
    {
        int numOverUndo = m_undoObjects.Count - m_undoPos;
        for (int i = 0; i < numOverUndo; i++)
        {
            GameObject gO = m_undoObjects[m_undoObjects.Count - 1].GetObj();
            if (gO.layer == LayerMask.NameToLayer("Path"))
                LineScript.AddToPathPool(gO);
            m_undoObjects.RemoveAt(m_undoObjects.Count - 1);
        }

        m_undoPos++;
        UndoItem uItem = new UndoItem(_gO);
        m_undoObjects.Add(uItem);
    }

    static public void AddToUndo(GameObject _gO, Vector2 _mousePos)
    {
        for (int i = 0; i < m_undoObjects.Count - m_undoPos; i++)
        {
            GameObject gO = m_undoObjects[m_undoObjects.Count - 1].GetObj();
            if (gO.layer == LayerMask.NameToLayer("Path"))
                LineScript.AddToPathPool(gO);
            m_undoObjects.RemoveAt(m_undoObjects.Count - 1);
        }

        m_undoPos++;
        UndoItem uItem = new UndoItem(_gO, _gO.transform.position);
        m_undoObjects.Add(uItem);
    }

    static public int GetUndoObjectsCount() { return m_undoObjects.Count; }

    static public int GetUndoPos() { return m_undoPos; }
}
