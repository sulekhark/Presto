
#define JT_LIMIT_1 500 
#define JT_LIMIT_2 2000
#define STUB_PROB 0.8

enum infType {
JT,
COMPUTE,
STUB
};

typedef struct {
    string methDir;
    size_t count;
    double defaultProb;
    string consFN;
    string probFN;
    string obsFN;
    int jtRetval;
} jtArgs_t;


typedef struct {
    string calleeMeth;
    set<string> frontiers;
    string calleeFrontUniqStr;
    map<string, double> observables;
} methPart_t;

typedef struct {
    string method;
    set<string> frontiers;
    string frontUniqStr;
    set<vector<string>> methClauses;
    map<string, double> clObs;
    map<string, double> observables;
    map<string, double> metaObs;
    list<methPart_t> computeParts;
    infType inferenceType;
} methInferenceInfo_t;


/********** EXTERN VARIABLES **********/

extern set<string> allTuples;
extern set<vector<string>> allClauses;
extern set<string> allInputTuples;
extern unordered_set<size_t> allTids;
extern unordered_set<size_t> allCids;

extern set<string> methTuples;
extern set<vector<string>> methClauses;
extern unordered_set<size_t> methInputTids;
extern set<size_t> methConsequentTids;
extern unordered_set<size_t> methTids;
extern unordered_set<size_t> methCids;

extern set<string> *workTuples;
extern set<vector<string>> *workClauses;
extern unordered_set<size_t> *workInputTids;
extern set<size_t> *workConsequentTids;
extern unordered_set<size_t> *workTids;
extern unordered_set<size_t> *workCids;

extern map<vector<string>, size_t> clause2Cid;
extern unordered_map<size_t, vector<string>> cid2Clause;
extern unordered_map<string, size_t> tuple2Tid;
extern unordered_map<size_t, string> tid2Tuple;

extern unordered_set<size_t> allInputTids;
extern set<size_t> allConsequentTids;
extern unordered_map<size_t, double> cid2Prob;
extern unordered_map<size_t, string> cid2RuleName;
extern unordered_map<string, unordered_set<size_t>> ruleName2Cids;
extern unordered_map<size_t,size_t> cid2ConsequentTid;
extern unordered_map<size_t, set<size_t>> cid2AntecedentTids;
extern unordered_map<size_t, set<size_t>> consequentTid2Cids;
extern unordered_map<size_t, set<size_t>> antecedentTid2Cids;

extern set<vector<string>> resultClauses;

extern double defaultRuleProb;
extern size_t generatedNameNdx;
extern size_t intermediateSaveFrequency;

/*********** EXTERN FUNCTIONS ************/

extern bool isSelfLoop(const vector<string>& clause);
extern string lit2Tuple(const string& literal);
extern vector<string> getAntecedents(const vector<string>& clause);
extern string getConsequent(const vector<string>& clause);

extern void printSet (string str, set<size_t> &s);
extern void printUSet (string str, unordered_set<size_t> &s);
extern void printSet (string str, set<string> &s);


extern void setWorkingAll();
extern void setWorkingMeth();
extern void clearFramework ();
extern void initializeFramework (set<vector<string>>& setOfClauses);
extern void initializeFramework (set<vector<string>>& setOfClauses, map<vector<string>, double>& clause2Prob);
extern void initializeFramework (string inConsFileName, string inRuleProbFileName);
extern void eraseClause(vector<string> clause);
extern void copyIpClausesToOp ();
extern void emitFromFramework (string outConsFileName, string outRuleProbFileName);
extern bool sanityCheck1 ();
extern bool sanityCheck2 ();
extern bool sanityCheck3 ();
