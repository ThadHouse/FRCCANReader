#pragma once

#include "../CANController.h"

#include "wpi/mutex.h"
#include <thread>
#include <atomic>
#include <vector>
#include "candle.h"
#include "wpi/ConcurrentQueue.h"

class WindowsMessagePump;

class WindowsCANController : public CANController
{
public:
  #if UNICODE
    typedef std::wstring string_type;
    typedef std::wstring_view string_view_type;
#else
    typedef std::string string_type;
    typedef std::string_view string_view_type;
#endif

    WindowsCANController();
    ~WindowsCANController() override;

    void selectDevice(std::string port);
    std::optional<CANData> getData() override;
    void releaseData() override;


    static std::vector<std::pair<string_type, string_type>> getDevices();

private:
    void readThreadMain();

    void disconnected();

    void connected();

    std::atomic_bool m_running;
    std::thread m_incomingThread;

    std::string m_path;

    std::atomic<candle_handle> m_candleHandle{nullptr};

    wpi::ConcurrentQueue<CANData> m_incomingData;
    std::unique_ptr<WindowsMessagePump> m_messagePump;
};
