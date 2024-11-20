#pragma once

#if defined(_WIN32)
#ifdef CPPPLAYER_EXPORTS
#define CPPPLAYER_API __declspec(dllexport)
#else
#define CPPPLAYER_API __declspec(dllimport)
#endif
#elif defined(__APPLE__)
#define CPPPLAYER_API __attribute__((visibility("default")))
#endif

// This class is exported from the dll
class CPPPLAYER_API CCppPlayer {
public:
    static CCppPlayer& GetInstance() {
        static CCppPlayer instance;
        return instance;
    }

    void Initialize(int myNumber, int column, int row);
    const char* GetName();
    int MoveNext(int argc, int* argv, int myPosition);

private:
    CCppPlayer(void) {}
    ~CCppPlayer() {}
};

extern "C" {
    CPPPLAYER_API void initialize(int myNumber, int column, int row) {
        CCppPlayer::GetInstance().Initialize(myNumber, column, row);
    }
    CPPPLAYER_API const char* getName() {
        return CCppPlayer::GetInstance().GetName();
    }
    CPPPLAYER_API int moveNext(int argc, int* argv, int myPosition) {
        return CCppPlayer::GetInstance().MoveNext(argc, argv, myPosition);
    }
}

