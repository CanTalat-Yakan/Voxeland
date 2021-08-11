using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayCaster : MonoBehaviour
{
    [SerializeField] PlayerInputHandler inputHandler;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] GameObject breakParticleFX;
    [SerializeField] GameObject cube;
    [SerializeField] Image radial;
    [SerializeField] Image selected;
    [SerializeField] Sprite[] icons;
    VoxelMaster master { get => GameManager.Instance.m_VoxelMaster; }
    GameObject selectionCube = null;
    Vector3Int? _lastVoxelPosInt = new Vector3Int();
    int currentID = 0;
    int lastID = 1;

    void Update()
    {
        if (GameManager.Instance.LOCKED) return;
        if (!GameManager.Instance.m_MainCamera) return;
        if (selectionCube is null) selectionCube = GameObject.Instantiate(cube, Vector3.zero, Quaternion.identity);

        DoVoxelSelectedID();

        RaycastHit hit = GameManager.Instance.HitRayCast(8);
        bool b = GameManager.Instance.BoolRayCast(8);
        selectionCube.SetActive(b);
        radial.gameObject.SetActive(b);
        if (!b) return;

        DoVoxelManipulation(
            hit.point - hit.normal * 0.5f,
            hit.point + hit.normal * 0.5f);

    }

    void DoVoxelSelectedID()
    {
        currentID += Mathf.FloorToInt(Input.mouseScrollDelta.y);
        currentID = currentID < 0 ? icons.Length - 1 : currentID >= icons.Length ? 0 : currentID;
        if (currentID != lastID)
            selected.sprite = icons[currentID];
        lastID = currentID;
    }

    void DoVoxelManipulation(Vector3 _voxelPos, Vector3 _airPos)
    {
        Vector3Int VoxelPosInt = Vector3Int.FloorToInt(_voxelPos);
        selectionCube.transform.position = VoxelPosInt;

        //Break Voxel
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || _lastVoxelPosInt != VoxelPosInt)
        {
            StopAllCoroutines();
            _lastVoxelPosInt = null;
            radial.gameObject.SetActive(false);
        }
        if (Input.GetMouseButton(0))
            if (_lastVoxelPosInt != VoxelPosInt)
                StartCoroutine(RemoveVoxel(_voxelPos));

        //Place Voxel
        if (Input.GetMouseButtonDown(1))
            if (NotInPlayer(_airPos))
                StartCoroutine(PlaceVoxel(_airPos));
    }


    bool NotInPlayer(Vector3 _p, bool _crouching = false)
    {
        Vector3 camPos = Vector3Int.FloorToInt(GameManager.Instance.m_MainCamera.gameObject.transform.position);
        Vector3 airBlock = Vector3Int.FloorToInt(_p);

        if (airBlock == camPos
            || (!_crouching && airBlock == Vector3.down + camPos))
            return false;

        foreach (Player clients in GameManager.Instance.ClientList)
            if (airBlock == Vector3Int.FloorToInt(clients.transform.position)
                || airBlock == Vector3Int.FloorToInt(Vector3.up + clients.transform.position))
                return false;

        return true;
    }

    IEnumerator PlaceVoxel(Vector3 _pos)
    {
        //Audio
        AudioManager.Instance.Play(AudioManager.PlayRandomFromList(ref AudioManager.Instance.m_AudioInfo.BlockPlaced[0].clips)).outputAudioMixerGroup = AudioManager.Instance.m_FXMixer;

        //Networking
        inputHandler.CmdSetVoxel((short)currentID, _pos);

        master.SetVoxelID(_pos, (short)currentID);
        master.FastRefresh();

        yield return null;
    }
    IEnumerator RemoveVoxel(Vector3 _pos)
    {
        Voxel voxel = master.GetVoxel(_pos);

        if (voxel != null)
        {
            //Durability Timer
            float startTime = Time.time;
            float endTime = startTime + voxel.Info.Durability;
            while (Time.time < endTime)
            {
                _lastVoxelPosInt = Vector3Int.FloorToInt(_pos);
                radial.gameObject.SetActive(true);
                radial.fillAmount = (Time.time - startTime) / voxel.Info.Durability;
                yield return new WaitForEndOfFrame();
            }

            //Audio Effect
            AudioManager.Instance.Play(AudioManager.PlayRandomFromList(ref AudioManager.Instance.m_AudioInfo.BlockRemoved[0].clips)).outputAudioMixerGroup = AudioManager.Instance.m_FXMixer;
            GameObject gobj = Instantiate(breakParticleFX, _pos, Quaternion.identity);
            Destroy(gobj, 6);

            //Particle System Effect
            var ps = gobj.GetComponent<ParticleSystemRenderer>();
            var main = ps.material;

            //Sprite of PS
            var sprite = icons[master.GetVoxelID(_pos)];
            var size = 1;
            var croppedTexture = new Texture2D((int)(sprite.rect.width * size), (int)(sprite.rect.height * size));
            var pixels = sprite.texture.GetPixels((int)(sprite.textureRect.x),
                                                  (int)(sprite.textureRect.y),
                                                  (int)(sprite.textureRect.width * size),
                                                  (int)(sprite.textureRect.height * size));
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            main.SetTexture("_BaseMap", croppedTexture);

            //Networking
            inputHandler.CmdSetVoxel((short)VoxelType.AIR, _pos);

            master.RemoveVoxelAt(_pos);
            master.FastRefresh();
        }

        yield return null;
    }





    // void OnGUI()
    // {
    //     if (!GameManager.Instance.m_ShowDebugInfo) return;


    //     int labelSpacing = 33;
    //     Rect rect = new Rect(Screen.width - 404, 0, 400, 30);
    //     Rect rect2 = new Rect(Screen.width - 404, Screen.height, 400, 30);

    //     var TextStyle = new GUIStyle();
    //     TextStyle.fontSize = 24;
    //     TextStyle.normal.textColor = Color.white;
    //     TextStyle.normal.background = Texture2D.grayTexture;
    //     TextStyle.font = GameManager.Instance.m_Settings.BlockFont;

    //     rect.y += 8;
    //     if (GameManager.Instance)
    //         GUI.Box(rect, " Player Pos: " + FloatToInt(GameManager.Instance.m_Player.gameObject.transform.position + Vector3.up * 0.02f), TextStyle);
    //     rect.y += labelSpacing;
    //     if (m_CurrentChunk != null)
    //         GUI.Box(rect, " Walking Chunk: " + (Vector3Int)m_CurrentChunk.chunkPos, TextStyle);
    //     else
    //         GUI.Box(rect, " Walking Chunk: None", TextStyle);
    //     rect.y += labelSpacing;

    //     if (m_TargetChunk != null)
    //         GUI.Box(rect, " Focused Chunk: " + (Vector3Int)m_TargetChunk.chunkPos, TextStyle);
    //     else
    //         GUI.Box(rect, " Focused Chunk: None", TextStyle);
    //     rect.y += labelSpacing;

    //     if (m_TargetBlockPos != null)
    //     {
    //         GUI.Box(rect, " Target Voxel: " + m_TargetVoxel.Value.IsSolid(), TextStyle);
    //         rect.y += labelSpacing;
    //         rect.x += labelSpacing;
    //         rect.width -= labelSpacing;
    //         GUI.Box(rect, " Voxel Pos: " + m_TargetBlockPos, TextStyle);
    //         rect.y += labelSpacing;
    //         rect.x += labelSpacing;
    //         rect.width -= labelSpacing;
    //         GUI.Box(rect, " Local: " + (Vector3Int)m_localVoxelPos, TextStyle);
    //         rect.x -= labelSpacing * 2;
    //         rect.width += labelSpacing * 2;
    //     }
    //     else
    //         GUI.Box(rect, " Target Voxel: None", TextStyle);
    //     rect.y += labelSpacing;



    //     string s = "";

    //     if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) > 0.5f) s = "North";
    //     else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) < -0.5f) s = "South";
    //     else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.5f) s = "East";
    //     else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.5f) s = "West";

    //     if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) > 0.34f)
    //     {
    //         if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.34f) s = "North East";
    //         else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.34f) s = "North West";
    //     }
    //     if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) < -0.34f)
    //     {
    //         if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.34f) s = "South East";
    //         else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.34f) s = "South West";
    //     }

    //     rect2.y -= 8;
    //     rect2.y -= labelSpacing;
    //     if (GameManager.Instance)
    //         GUI.Box(rect2, " Compass: " + s, TextStyle);
    //     rect2.y -= labelSpacing;
    // }
}
