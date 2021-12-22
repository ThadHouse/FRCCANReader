#pragma once

#include "CANController.h"
#include "CANControllerDetector.h"

#include "wpi/mutex.h"
#include <thread>
#include <atomic>
#include <vector>
#include "candle.h"

class WindowsMessagePump;

class WindowsCANController : public CANController
{
public:
    WindowsCANController();
    ~WindowsCANController() override;

    void SelectDevice(std::string_view port) override;

    static std::vector<can::CANDevice> EnumerateDevices();

private:
    void readThreadMain();

    void disconnected();

    void connected();

    std::atomic_bool m_running;
    std::thread m_incomingThread;

    std::string m_path;

    std::atomic<candle_handle> m_candleHandle{nullptr};

    std::unique_ptr<WindowsMessagePump> m_messagePump;
};
