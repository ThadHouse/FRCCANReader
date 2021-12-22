#include "WindowsCANController.h"

#include "candle.h"
#include <wpi/timestamp.h>
#include <cstring>
#include <wpi/ConcurrentQueue.h>
#include "WindowsMessagePump.h"

#include <Dbt.h>
#include <ks.h>
#include <ksmedia.h>

static void openHandle(candle_handle handle)
{
    candle_dev_open(handle);
    candle_channel_set_bitrate(handle, 0, 1000000);
    candle_channel_start(handle, 0, CANDLE_MODE_NORMAL);
    printf("Starting Device\n");
}

WindowsCANController::WindowsCANController()
{
    m_messagePump = std::make_unique<WindowsMessagePump>([this](HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam) {
        printf("Received Message: %08x\n", uiMsg);

        switch (uiMsg)
        {
        case WM_DEVICECHANGE:
            PDEV_BROADCAST_HDR parameter = reinterpret_cast<PDEV_BROADCAST_HDR>(lParam);

            if (m_path.empty() && wParam == DBT_DEVICEARRIVAL && parameter->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
            {
                // First time device connecting, connect to it.
                DEV_BROADCAST_DEVICEINTERFACE *pDi2 = reinterpret_cast<DEV_BROADCAST_DEVICEINTERFACE *>(parameter);
                const char *str = pDi2->dbcc_name;
                SelectDevice(str);
                printf("Dev Name %s\n", pDi2->dbcc_name);
                break;
            }

            if (parameter->dbch_devicetype != DBT_DEVTYP_DEVICEINTERFACE)
            {
                break;
            }

            DEV_BROADCAST_DEVICEINTERFACE *pDi = reinterpret_cast<DEV_BROADCAST_DEVICEINTERFACE *>(parameter);

            if (_stricmp(m_path.c_str(), pDi->dbcc_name) == 0)
            {
                if (wParam == DBT_DEVICEARRIVAL)
                {
                    connected();
                }
                else if (wParam == DBT_DEVICEREMOVECOMPLETE)
                {
                    disconnected();
                }
            }
        }
        return DefWindowProc(hwnd, uiMsg, wParam, lParam);
    });

    candle_list_handle listHandle;
    if (!candle_list_scan(&listHandle))
    {
        return;
    }

    uint8_t length = 0;
    if (!candle_list_length(listHandle, &length))
    {
        candle_list_free(listHandle);
        return;
    }

    if (length > 0)
    {
        candle_handle handle;
        candle_dev_get(listHandle, 0, &handle);
        m_path = candle_dev_get_path(handle);
        openHandle(handle);
        m_candleHandle = handle;
    }

    candle_list_free(listHandle);

    m_running = true;
    m_incomingThread = std::thread(&WindowsCANController::readThreadMain, this);
}

void WindowsCANController::connected()
{
    candle_list_handle listHandle;
    if (!candle_list_scan(&listHandle))
    {
        return;
    }

    uint8_t length = 0;
    if (!candle_list_length(listHandle, &length))
    {
        candle_list_free(listHandle);
        return;
    }

    for (uint16_t i = 0; i < length; i++)
    {
        candle_handle handle;
        if (!candle_dev_get(listHandle, i, &handle))
        {
            continue;
        }

        auto path = candle_dev_get_path(handle);
        if (_stricmp(m_path.c_str(), path) == 0)
        {
            openHandle(handle);
            m_candleHandle = handle;
            break;
        }
    }

    candle_list_free(listHandle);
}

void WindowsCANController::disconnected()
{
    auto candleHandle = m_candleHandle.exchange(nullptr);

    if (!candleHandle)
        return;

    candle_channel_stop(candleHandle, 0);
    candle_dev_close(candleHandle);
    candle_dev_free(candleHandle);
}

WindowsCANController::~WindowsCANController()
{
    m_running = false;
    m_incomingThread.join();
}

void WindowsCANController::SelectDevice(std::string_view port)
{
    if (port == m_path) {
        return;
    }
    m_path = port;
    disconnected();
    connected();
}


std::vector<can::CANDevice> WindowsCANController::EnumerateDevices()
{
    candle_list_handle listHandle;
    if (!candle_list_scan(&listHandle))
    {
        return {};
    }

    uint8_t length = 0;
    if (!candle_list_length(listHandle, &length))
    {
        candle_list_free(listHandle);
        return {};
    }

    std::vector<can::CANDevice> paths;

    for (uint16_t i = 0; i < length; i++)
    {
        candle_handle handle;
        if (!candle_dev_get(listHandle, i, &handle))
        {
            continue;
        }

        auto path = candle_dev_get_path(handle);
        auto name = candle_dev_get_name(handle);
        paths.emplace_back(can::CANDevice{name, path});
        candle_dev_free(handle);
    }

    candle_list_free(listHandle);

    return paths;
}

void WindowsCANController::readThreadMain()
{
    candle_frame_t frame;

    while (m_running)
    {
        auto handle = m_candleHandle.load();
        if (!handle)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
            continue;
        }
        if (!candle_frame_read(handle, &frame, 1000))
        {
            continue;
        }

        CANData newData;
        newData.id = candle_frame_id(&frame);

        auto dlc = candle_frame_dlc(&frame);
        auto data = candle_frame_data(&frame);

        int i = 0;
        for (; i < dlc; i++)
        {
            newData.data[i] = data[i];
        }

        for (; i < 8; i++)
        {
            newData.data[i] = 0;
        }

        newData.length = dlc;

        newData.timestamp = wpi::Now();

        PushData(std::move(newData));
    }
}
