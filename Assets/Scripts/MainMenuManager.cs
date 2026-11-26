using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk pindah Scene

public class MainMenuManager : MonoBehaviour
{
    public void KlikNewGame()
    {
        // Ubah tulisan "SampleScene" menjadi "Scene" sesuai dengan yang ada di daftar
        SceneManager.LoadScene("Scene");
        
        // Trik tambahan: Anda juga bisa memanggilnya menggunakan angka Index-nya langsung (anti-typo)
        // SceneManager.LoadScene(1);
    }

    public void KlikContinue()
    {
        // Sesuai GDD, ini nanti untuk Load Slot. Sekarang kita beri pesan log dulu.
        Debug.Log("Fitur Load Game ditekan!");
    }

    public void KlikExit()
    {
        Debug.Log("Game Keluar!");
        Application.Quit(); // Perintah ini hanya berfungsi saat game sudah di-build (di-export)
    }
}