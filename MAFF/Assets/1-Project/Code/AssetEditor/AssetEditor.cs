using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetEditor : MonoBehaviour
    {
        private AssetModifier modifier;

        [Header("Links")]
        [SerializeField]
        private AssetReference[] assets;

        [SerializeField]
        private Transform assetParent;

        [SerializeField]
        private Transform buttonParent;

        [SerializeField]
        private Button buttonPreset;

        [Header("Options")]
        [SerializeField]
        private float rotationSpeed = 5.0f;

        [SerializeField]
        private float zoomSpeed = 10.0f;

        private void Start()
        {
            modifier = GetComponent<AssetModifier>();

            for (int i = 0; i < assets.Length; i++)
            {
                int c_i = i;
                CreateButton($"Instantiate {assets[i].RuntimeKey}")
                    .onClick.AddListener(() => DownloadAndInstantiate(assets[c_i]));
            }

            CreateButton($"Check For Update")
                .onClick.AddListener(() => CheckForUpdate());

            CreateButton($"Check For Download")
                .onClick.AddListener(() => CheckForDownload());
        }

        void Update()
        {
            OnMouseInput();
        }

        private void OnMouseInput()
        {
            var cameraTransform = Camera.main.transform;

            // 우클릭(마우스 오른쪽 버튼)이 눌린 상태에서 처리
            if (Input.GetMouseButton(1))
            {
                // 마우스 이동 값을 가져옵니다.
                float horizontal = Input.GetAxis("Mouse X") * rotationSpeed;
                float vertical = Input.GetAxis("Mouse Y") * rotationSpeed;

                // 원점을 기준으로 Y축을 따라 수평 회전
                cameraTransform.RotateAround(Vector3.zero, Vector3.up, horizontal);

                // 원점을 기준으로 카메라의 오른쪽 축을 따라 수직 회전
                // 음수를 곱해 위/아래 방향이 자연스럽게 움직이도록 함
                cameraTransform.RotateAround(Vector3.zero, cameraTransform.right, -vertical);
            }

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // 마우스 휠 입력을 통한 확대/축소 처리
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 카메라와 원점 사이의 방향 벡터 계산
                Vector3 direction = (cameraTransform.position - Vector3.zero).normalized;
                // 현재 카메라와 원점 사이의 거리
                float distance = cameraTransform.position.magnitude;
                // 스크롤 값에 따라 거리를 변경
                distance -= scroll * zoomSpeed;
                // 최소 거리가 1m 이하로 내려가지 않도록 보정
                distance = Mathf.Clamp(distance, 1f, 20f);
                // 변경된 거리로 카메라 위치 갱신
                cameraTransform.position = direction * distance;
            }
        }

        private void DownloadAndInstantiate(AssetReference asset)
        {
            for (int i = assetParent.childCount - 1; i >= 0; i--)
            {
                Destroy(assetParent.GetChild(i).gameObject);
            }

            Addressables.GetDownloadSizeAsync(asset)
                .Completed += handle =>
                {
                    if (handle.Result > 0)
                    {
                        MessageBox.Show(
                            $"Total Download Size: {handle.Result}\n" +
                            "Do you want to download?",
                            eMessageBoxButtons.YesNo)
                        .Subscribe(result =>
                        {
                            if (result.Code == eMessageBoxResult.Yes)
                            {
                                Addressables.DownloadDependenciesAsync(asset)
                                .Completed += _ => Addressables.InstantiateAsync(asset, assetParent)
                                .Completed += handle => modifier.SetFbxInstance(handle.Result);
                            }
                        });
                    }
                    else
                    {
                        Addressables.InstantiateAsync(asset, assetParent)
                        .Completed += handle => modifier.SetFbxInstance(handle.Result);
                    }
                };
        }

        private void CheckForUpdate()
        {
            Addressables.CheckForCatalogUpdates()
            .Completed += handle =>
            {
                if (handle.Result.Count > 0)
                {
                    MessageBox.Show(
                        "Asset Update Available.\n" +
                        "Do you want to check for download?",
                        eMessageBoxButtons.YesNo)
                    .Subscribe(result =>
                    {
                        if (result.Code == eMessageBoxResult.Yes)
                        {
                            Addressables.UpdateCatalogs(handle.Result)
                            .Completed += _ => CheckForDownload();
                        }
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Asset is the latest version.",
                        eMessageBoxButtons.OK);
                }
            };
        }

        private void CheckForDownload()
        {
            Addressables.GetDownloadSizeAsync(assets)
            .Completed += handle =>
            {
                if (handle.Result > 0)
                {
                    MessageBox.Show(
                        $"Total Download Size: {handle.Result}\n" +
                        "Do you want to download?",
                        eMessageBoxButtons.YesNo)
                    .Subscribe(result =>
                    {
                        if (result.Code == eMessageBoxResult.Yes)
                        {
                            Addressables.DownloadDependenciesAsync(assets, Addressables.MergeMode.Union);
                        }
                    });
                }
                else
                {
                    MessageBox.Show(
                        "All asset downloaded for current version",
                        eMessageBoxButtons.OK);
                }
            };
        }

        private Button CreateButton(string text)
        {
            var button = Instantiate(buttonPreset, buttonParent);
            button.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = text;

            return button;
        }
    }
}