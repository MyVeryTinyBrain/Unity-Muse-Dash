using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NoteSubHandle : NoteComponent
{
    protected Mediator mediator => Mediator.i;

    public const string subHandleName = "SubHandle";

    public float width = 0.5f;
    public Color color = new Color(1, 1, 1, 0.5f);
    public Color subColor = Color.white;
    GameObject subHandleGameObject = null;
    Material glMaterial = null;

    public GameObject subHandleObject => subHandleGameObject;

    protected override void OnInit(Note note)
    {
        subHandleGameObject = new GameObject(subHandleName);
        subHandleGameObject.transform.SetParent(note.transform);

        Shader shader = Shader.Find("GL/GL");
        glMaterial = new Material(shader);
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterial.SetInt("_ZWrite", 0);

        glMaterial.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
        glMaterial.SetInt("_Stencil", 0);
        glMaterial.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
    }

    protected override void OnPostInit()
    {

    }

    private void OnDestroy()
    {
        if(subHandleGameObject == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach(Transform child in allChildren)
            {
                if(child.gameObject.name == subHandleName)
                {
                    subHandleGameObject = child.gameObject;
                    break;
                }
            }
        }

        Destroy(subHandleGameObject);
        subHandleGameObject = null;
    }

    private void Update()
    {
        float x = note.ratio * mediator.gameSettings.lengthPerSeconds;
        float y = note.GetHitLocalY();
        Vector2 hitPointLocalPos = new Vector2(x, y);
        Vector2 worldPos = mediator.hitPoint.transform.TransformPoint(hitPointLocalPos);
        subHandleGameObject.transform.position = worldPos;
    }

    private void OnRenderObject()
    {
        if (!note.enabled)
        {
            return;
        }

        if (!subHandleGameObject.activeInHierarchy)
        {
            return;
        }

        glMaterial.SetPass(0);

        GL.MultMatrix(subHandleGameObject.transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        {
            float hw = width * 0.5f;

            GL.Color(color * subColor);

            GL.Vertex3(-hw, +hw, 0);
            GL.Vertex3(+hw, -hw, 0);

            GL.Vertex3(-hw, -hw, 0);
            GL.Vertex3(+hw, +hw, 0);
        }
        GL.End();
    }
}
