#include <boost/algorithm/string/predicate.hpp>
using boost::starts_with;

#include <algorithm>
#include <cassert>
#include <chrono>
#include <ctime>
#include <iostream>
#include <iterator>
#include <limits>
#include <map>
#include <set>
#include <string>
#include <utility>
#include <unordered_map>
#include <unordered_set>
#include <vector>
#include <stack>
#include <fstream>
using namespace std;

#include "util.h"
#include "framework.h"
#include "dfs_cycle_elim.h"


int main(int argc, char *argv[]) {

    clog << __LOGSTR_FULL__ << "Starting DFS-based cycle elimination." << endl;
    if (argc < 5) {
        cerr << __LOGSTR__ << "Insufficient arguments!" << endl
                           << "./dfs_cycle_elim inConsFileName outConsFileName inRuleProbFileName outRuleProbFileName" << endl;
        return 1;
    }
    string inConsFileName = argv[1];
    string outConsFileName = argv[2];
    string inRuleProbFileName = argv[3];
    string outRuleProbFileName = argv[4];

    setWorkingAll();
    initializeFramework(inConsFileName, inRuleProbFileName);
    eliminateCycles();
    sanityCheck1();
    emitFromFramework(outConsFileName, outRuleProbFileName);
    return 0;
}
