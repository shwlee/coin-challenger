#include "CppPlayer.h"
#include <cstdlib>

void CCppPlayer::Initialize(int myNumber, int column, int row)
{
}

const char* CCppPlayer::GetName()
{
    return "ab";
}

int CCppPlayer::MoveNext(int argc, int* argv, int myPosition)
{
    return std::rand() % 4;
}
