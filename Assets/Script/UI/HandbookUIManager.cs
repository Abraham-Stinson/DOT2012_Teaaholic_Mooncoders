using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandbookUIManager : MonoBehaviour
{
    public List<GameObject> pages;
    public Button nextButton;
    public Button prevButton;
    public Button firstPageButton;

    public GameObject[] extraButtonsFirstPage;

    private int currentPageIndex = 0;

    void Start()
    {
        UpdatePage();
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        firstPageButton.onClick.AddListener(GoToFirstPage);
    }

    void UpdatePage()
    {
        SoundManager.Instance.PageSwitch();
        for (int i = 0; i < pages.Count; i++)
            pages[i].SetActive(i == currentPageIndex);

        nextButton.gameObject.SetActive(currentPageIndex < pages.Count - 1);
        prevButton.gameObject.SetActive(currentPageIndex > 0);
        firstPageButton.gameObject.SetActive(currentPageIndex > 0);

        foreach (var btn in extraButtonsFirstPage)
            btn.SetActive(currentPageIndex == 0);
    }

    public void NextPage()
    {
        SoundManager.Instance.PageSwitch();
        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        SoundManager.Instance.PageSwitch();
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePage();
        }
    }

    public void GoToFirstPage()
    {
        SoundManager.Instance.PageSwitch();
        currentPageIndex = 0;
        UpdatePage();
    }

    public void GoToPage(int index)
    {
        SoundManager.Instance.PageSwitch();
        if (index >= 0 && index < pages.Count)
        {
            currentPageIndex = index;
            UpdatePage();
        }
    }
}
