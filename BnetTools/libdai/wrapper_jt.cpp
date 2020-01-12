#include <dai/alldai.h>
using namespace dai;

#include <boost/lexical_cast.hpp>

#include <cassert>
#include <chrono>
#include <ctime>
#include <iostream>
#include <map>
#include <string>
#include <vector>
using namespace std;

static FactorGraph fg;
static PropertySet opts;
static BP bp;
static JTree jt;
static map<int, bool> clamps;
static bool doJTree;

void initBP() {
    bp = BP(fg, opts);
    for (const auto& clamp : clamps) {
        int varIndex = clamp.first;
        bool varValue = clamp.second;
        bp.clamp(varIndex, varValue ? 1 : 0);
    }
    bp.init();
}

void initJTree() {
    jt = JTree(fg, opts);
    jt.init();
}

void queryVariable() {
    int varIndex;
    cin >> varIndex;
    clog << __LOGSTR__ << "Q " << varIndex << endl;

    if (doJTree) {
        auto ans = jt.belief(fg.var(varIndex)).get(1);
        auto ans0 = jt.belief(fg.var(varIndex)).get(0);
        clog << __LOGSTR__ << "Returning " << ans << "." << " ans0 is: " << ans0 << endl;
        cout << ans << endl;
    } else {
        // auto ans = bp.belief(fg.var(varIndex)).get(1);
        auto ans = bp.newBelief(varIndex);
        clog << __LOGSTR__ << "Returning " << ans << "." << endl;
        cout << ans << endl;
    }
}

void queryFactor() {
    int factorIndex, valueIndex;
    cin >> factorIndex >> valueIndex;
    clog << __LOGSTR__ << "FQ " << factorIndex << " " << valueIndex << endl;

    if (doJTree) {
        auto ans = jt.beliefF(factorIndex).get(valueIndex);
        clog << __LOGSTR__ << "Returning " << ans << "." << endl;
        cout << ans << endl;
    } else {
        auto ans = bp.beliefF(factorIndex).get(valueIndex);
        clog << __LOGSTR__ << "Returning " << ans << "." << endl;
        cout << ans << endl;
    }
}

void runBP() {
    double tolerance;
    size_t minIters, maxIters, histLength;
    cin >> tolerance >> minIters >> maxIters >> histLength;
    clog << __LOGSTR__ << "BP " << tolerance << " " << minIters << " " << maxIters << " " << histLength << endl;

    double yetToConvergeFraction = bp.run(tolerance, minIters, maxIters, histLength);
    cout << yetToConvergeFraction << endl;
}

void runJTree() {
    clog << __LOGSTR__ << "JT " << endl;
    int cmdSuccess;
    try {
        jt.run();
    } catch( Exception &e ) {
        cmdSuccess = 1;
        clog << __LOGSTR__ << "JT Exception." << endl;
        cout << cmdSuccess << endl;
    }
    clog << __LOGSTR__ << "JT Done." << endl;
    cmdSuccess = 0;
    cout << cmdSuccess << endl;
}

void clamp() {
    int varIndex;
    string varValueStr;
    cin >> varIndex >> varValueStr;
    assert(varValueStr == "true" || varValueStr == "false");
    clog << __LOGSTR__ << "O " << varIndex << " " << varValueStr << endl;

    bool varValue = (varValueStr == "true");
    clamps[varIndex] = varValue;
    assert(doJTree == false);
    initBP();
    cout << "O " << varIndex << " " << varValueStr << endl;
}

int main(int argc, char *argv[]) {
    if (argc < 3) {
        cerr << __LOGSTR__ << "Insufficient number of arguments." << endl;
        return 1;
    }
    char *factorGraphFileName = argv[1];
    string inferType = argv[2];
    if (inferType == "jtree") {
        doJTree = true;
    } else {
        doJTree = false;
    }

    clog << __LOGSTR__ << "Hello!" << endl
                       << "wrapper.cpp compiled on " << __DATE__ << " at " << __TIME__ << "." << endl;
    fg.ReadFromFile(factorGraphFileName);
    clog << __LOGSTR__ << "Finished reading factor graph." << endl;

    opts.set("maxiter", static_cast<size_t>(10000000));
    opts.set("maxtime", Real(57600));
    opts.set("tol", Real(1e-6));
    opts.set("verb", static_cast<size_t>(1));

    if (doJTree == false) {
        opts.set("updates", string("SEQRND")); // "SEQRND", or "PARALL", or "SEQFIX", or "SEQRNDPAR"
        opts.set("logdomain", true);
        initBP();
    } else {
        opts.set("updates", string("HUGIN"));
        size_t maxstates = 1000000;
        try {
            boundTreewidth(fg, &eliminationCost_MinFill, maxstates);
        } catch( Exception &e ) {
            if( e.getCode() == Exception::OUT_OF_MEMORY )
                clog << __LOGSTR__ << "Cannot do junction tree inference (need more than " << maxstates << " states)." << endl;
            else
                clog << __LOGSTR__ << "Hit an exception: " << e.getCode() << endl;
            int retval = 1;
            cout << retval << endl;
            return retval;
        }
        initJTree(); 
    }

    string cmdType;
    while (cin >> cmdType) {
        clog << __LOGSTR__ << "Read command " << cmdType << endl;
        if (cmdType == "Q") {
            queryVariable();
        } else if (cmdType == "FQ") {
            queryFactor();
        } else if (cmdType == "BP") {
            if (doJTree == true) {
                cout << __LOGSTR__ << "Expected inference command JT. Ignoring command BP." << endl;
            } else {
                runBP();
            }
        } else if (cmdType == "JT") {
            if (doJTree == false) {
                cout << __LOGSTR__ << "Expected inference command BP. Ignoring command JT." << endl;
            } else {
                runJTree();
            }
        } else if (cmdType == "O") {
            clamp();
        } else {
            assert(cmdType == "NL");
            cout << endl;
        }
    }

    clog << __LOGSTR__ << "Bye!" << endl;
    return 0;
}
