#include <chrono>
#include <ctime>
#include <iostream>
#include <string>
using namespace std;

string nowstr() {
    auto today = chrono::system_clock::now();
    time_t tt = chrono::system_clock::to_time_t(today);
    string ans = ctime(&tt);
    return ans.substr(0, ans.size() - 1);
}
