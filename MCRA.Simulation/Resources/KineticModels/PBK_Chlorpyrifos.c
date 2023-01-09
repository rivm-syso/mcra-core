/*===============================================================================
 * Description: Kinetic model implementation for Chlorpyrifos and metabolites for R/deSolve.
 * Compounds: CPF, CPF-oxon, TCPy
 * Exposure: Oral Single/repeated dose
 * Species: Human
 * Made by: Shensheng Zhao and Anne Zwartsen
 * Organisation: RIVM
 * Version: DRAFT
 * C-code Waldo de Boer
 *===============================================================================
 */

#include <R.h>

/* Model variables: States */
#define Ast1	0
#define Ast2	1
#define ASP 	2
#define ARP 	3
#define AFP 	4
#define ALP 	5
#define ACPFO 	6
#define ATCPyA	7
#define ACLP  	8
#define AKP   	9
#define AMP   	10
#define AUP   	11
#define AHP   	12
#define ALuP  	13
#define ABrbP 	14
#define ABrtP 	15
#define AAP 	16
#define AVP 	17
#define ASM1	18
#define ARM1	19
#define AFM1	20
#define ALM1	21
#define ATCPyB  22
#define ACLM1   23
#define AKM1    24
#define AMM1    25
#define AUM1    26
#define AHM1    27
#define ALuM1   28
#define AAM1    29
#define AVM1    30
#define ATCPyC  31
#define ASM2 	32
#define ARM2 	33
#define AFM2 	34
#define ALM2 	35
#define ACLM2 	36
#define AKM2    37
#define AMM2    38
#define AUM2    39
#define AHM2    40
#define ALuM2 	41
#define ABrbM2	42
#define ABrtM2	43
#define	AVM2 	44
#define AAM2  	45
#define ABrbM1	46
#define ABrtM1	47

/* Model variables: Outputs */

/* Chlorpyrifos-oxon */
#define O_CVM1  0
#define O_CPM1  1

/* TCPy */
#define O_CVM2  2
#define O_CPM2  3

#define O_CVP   4
#define O_CPP   5
#define O_ACLM2 6

/* Model variables: Inputs */
static double parms[66];
#define	VLc 				parms[0]
#define	VFc 				parms[1]
#define	VLuc 				parms[2]
#define	VAc 				parms[3]
#define	VVc 				parms[4]
#define	VKc					parms[5]
#define	VMc					parms[6]
#define	VUc					parms[7]
#define	VBrc				parms[8]
#define	VHc 				parms[9]
#define	QLc					parms[10]
#define	QFc					parms[11]
#define	QKc					parms[12]
#define	QMc					parms[13]
#define	QUc					parms[14]
#define	QBrc				parms[15]
#define	QHc					parms[16]
#define	MWP					parms[17]
#define	MWM1				parms[18]
#define	MWM2				parms[19]
#define	LogPP				parms[20]
#define	LogPM1			    parms[21]
#define	LogPM2			    parms[22]
#define	Fa					parms[23]
#define	KaS					parms[24]
#define	KaI					parms[25]
#define	KsI					parms[26]
#define	fuP					parms[27]
#define	fuM1				parms[28]
#define	fuM2				parms[29]
#define	BPP					parms[30]
#define	BPM1				parms[31]
#define	BPM2				parms[32]
#define	KurineP				parms[33]
#define	KurineM1			parms[34]
#define	KurineM2			parms[35]
#define	CYPabundanceCYP1A2	parms[36]
#define	CYPabundanceCYP2B6	parms[37]
#define	CYPabundanceCYP2C19	parms[38]
#define	CYPabundanceCYP3A4	parms[39]
#define	ISEFCYP1A2			parms[40]
#define	ISEFCYP2B6			parms[41]
#define	ISEFCYP2C19			parms[42]
#define	ISEFCYP3A4			parms[43]
#define	MPL					parms[44]
#define	VMaxCYP1A2mP1		parms[45]
#define	VMaxCYP2B6mP1		parms[46]
#define	VMaxCYP2C19mP1		parms[47]
#define	VMaxCYP3A4mP1		parms[48]
#define	KmCYP1A2P1			parms[49]
#define	KmCYP2B6P1			parms[50]
#define	KmCYP2C19P1			parms[51]
#define	KmCYP3A4P1			parms[52]
#define	VMaxCYP1A2mP2		parms[53]
#define	VMaxCYP2B6mP2		parms[54]
#define	VMaxCYP2C19mP2	    parms[55]
#define	VMaxCYP3A4mP2		parms[56]
#define	KmCYP1A2P2			parms[57]
#define	KmCYP2B6P2			parms[58]
#define	KmCYP2C19P2			parms[59]
#define	KmCYP3A4P2			parms[60]
#define	VMax3c				parms[61]
#define	Km3					parms[62]
#define	VMax4c				parms[63]
#define	Km4					parms[64]
#define	BW					parms[65]

/* Forcing (Input) functions */
static double forc[1];

/* Internal model input parameters*/

/* Tissue fractions of BW (Brown 1997) */
static double VRc; /*Fraction richly perfused tissue of BW */
static double VSc; /*Fraction blood flow to slowly perfused tissue of BW */

/* Tissue volumes (kg) */
static double VL; /*Volume of liver in kg */
static double VF; /*Volume of fat in kg */
static double VLu; /*Volume of lungs in kg */
static double VK; /*Volume of kidneys in kg */
static double VM; /*Volume of muscle in  */
static double VU; /*Volume of uterus in kg*/
static double VBr; /*Volume of brain in kg*/
static double VBrb; /*Volume of brain blood in kg*/
static double VBrt; /*Volume of brain tissue in kg*/
static double VH; /*Volume of heart in kg*/
static double VR; /*Volume of richly perfused tissue in kg*/
static double VS; /*Volume of slowly perfused tissue in kg*/
static double VA; /*Volume of arterial blood in kg*/
static double VV; /*Volume of venous blood in kg  */

 /* Blood flow rate fractions (Brown 1997) */
static double QSc; /*Fraction of blood flow to slowly perfused tissue */
static double QRc; /*Fraction of blood flow to rapidly perfused tissue */

 /* Fraction of blood flow to rapidly perfused tissue. Blood flow rates */
static double QC; /* = 15 * pow(BW, 0.74) */
static double QLu; /*Blood flow rate to lung in L/h*/
static double QL; /*Blood flow rate to liver in L/h*/
static double QF; /*Blood flow rate to fat in L/h*/
static double QK; /*Blood flow rate to kidneys in L/h*/
static double QM; /*Blood flow rate to muscle in L/h*/
static double QU; /*Blood flow rate to uterus in L/h*/
static double QBr; /*Blood flow rate to brain in L/h */
static double QH ; /*Blood flow rate to heart in L/h*/
static double QR; /*Blood flow rate to richly perfused tissue in L/h*/
static double QS; /*Blood flow rate to slowly perfused tissue in L/h*/

/*
 * ===============================================================================
 * Physicochemical parameters
 * ===============================================================================
 */

/* Partition coefficient Parent (Predicted by WFSR tool) */
static double PLP; /*Liver/blood partition coefficient	*/
static double PFP; /*Fat/blood partition coefficient */
static double PMP; /*Muscle/blood partition coefficient*/
static double PUP; /*Uterus/blood partition coefficient*/
static double PBrP; /*Brain/blood partition coefficient*/
static double PRP; /*Richly perfused tissues/blood partition coefficient */
static double PSP; /*Slowly perfused tissues/blood partition coefficient */
static double PLuP; /*Lung/blood partition coefficient */
static double PKP; /*Kidney/blood partition coefficient */
static double PHP ; /*Heart/blood partition coefficient */

/* Partition coefficient Metabolite 1(Predicted by WFSR tool) */
static double PLM1; /*Liver/blood partition coefficient	*/
static double PFM1; /*Fat/blood partition coefficient */
static double PMM1; /*Muscle/blood partition coefficient*/
static double PUM1; /*Uterus/blood partition coefficient*/
static double PBrM1; /*Brain/blood partition coefficient*/
static double PRM1; /*Richly perfused tissues/blood partition coefficient */
static double PSM1; /*Slowly perfused tissues/blood partition coefficient */
static double PLuM1; /*Lung/blood partition coefficient */
static double PKM1; /*Kidney/blood partition coefficient */
static double PHM1; /*eart/blood partition coefficient */

/* Partition coefficient Metabolite 2 (Predicted by WFSR tool) */
static double PLM2; /*Liver/blood partition coefficient	*/
static double PFM2; /*Fat/blood partition coefficient */
static double PMM2; /*Muscle/blood partition coefficient*/
static double PUM2; /*Uterus/blood partition coefficient*/
static double PBrM2; /*Brain/blood partition coefficient*/
static double PRM2; /*Richly perfused tissues/blood partition coefficient */
static double PSM2; /*Slowly perfused tissues/blood partition coefficient */
static double PLuM2; /*Lung/blood partition coefficient */
static double PKM2; /*Kidney/blood partition coefficient */
static double PHM2 ; /*Heart/blood partition coefficient */

/*
 * ===============================================================================
 * Biochemical parameters (various)
 * ===============================================================================
 */

/* Brain Permeability surface area product */
static double PSBrP; 	  /*PS Brain parent compound in L/h */
static double PSBrM1; 	/*PS Brain metabolite 1 in L/h */
static double PSBrM2; 	/*PS Brain metabolite 2 in L/h */

/*
 * ===============================================================================
 * Biochemical parameters (metabolism)
 * ===============================================================================
 */

/* Pathway 1 (liver): CPF-->CPF-oxon. Vmax Pathway 1 */
static double VMaxCYP1A2cP1;    /* Vmax of CYP1A2 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP2B6cP1;    /* Vmax of CYP2B6 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP2C19cP1;   /* Vmax of CYP2C19 at microsomal level (pmol/min/mg microsomal protein)	*/
static double VMaxCYP3A4cP1;    /* Vmax of CYP3A4 at microsomal level (pmol/min/mg microsomal protein)	*/
static double VMaxCYP1A2P1;     /* Vmax of CYP1A2 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))    */
static double VMaxCYP2B6P1;     /* Vmax of CYP2B6 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))  */
static double VMaxCYP2C19P1;    /* Vmax of CYP2C19 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))  */
static double VMaxCYP3A4P1;     /* Vmax of CYP3A4 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))  */

/* Pathway 2 (liver): CPF-->TCPy. Vmax Pathway 2 */
static double VMaxCYP1A2cP2;    /* Vmax of CYP1A2 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP2B6cP2;    /* Vmax of CYP2B6 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP2C19cP2;   /* Vmax of CYP2C19 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP3A4cP2;    /* Vmax of CYP3A4 at microsomal level (pmol/min/mg microsomal protein)*/
static double VMaxCYP1A2P2;     /* Vmax of CYP1A2 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))  */
static double VMaxCYP2B6P2;     /* Vmax of CYP2B6 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver)) */
static double VMaxCYP2C19P2;    /* Vmax of CYP2C19 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver)) */
static double VMaxCYP3A4P2;     /* Vmax of CYP3A4 at tissue level (umol/hr/liver) (1000000-->pmol to umol;  60-->min to hour; 1000-->g to kg (liver))  */

/* Pathway 3 (liver): CPF-oxon-->TCPy. Vmax Pathway 3*/
static double VMax3;            /*Scaled maximum rate of metabolism (umol/hr/liver per person) (1000->nmol to umol; 60-->min to hour; 1000-->g to kg (liver))*/

/* Pathway 4 (blood): CPFO-->TCPy. Vmax Pathway 4*/
static double VMax4; /*Scaled maximum rate of metabolism (umol/hr/whole blood per person) (1000-->nmol to umol; 60-->min to hour; 1000-->g to kg (blood))*/

/* Internal model derivative section */
static double CAP;
static double CSP;
static double CVSP;
static double CVRP;
static double CVMP;
static double CVUP;
static double CVHP;
static double CFP;
static double CLP;
static double CVLP;
static double CVFP;
static double CVKP;
static double CRP;
static double CKP;
static double CMP;
static double CUP;
static double CHP;
static double CVP;
static double CALuP;
static double CLuP;
static double CVBrP;
static double CVLM1;
static double CBrtP;
static double CBrbP;
static double ABrP;
static double CBrP;
static double CPP;
static double CAM1;
static double CSM1;
static double CVRM1;
static double CVSM1;
static double CRM1;
static double CVFM1;
static double CFM1;
static double CLM1;
static double CVKM1;
static double CVMM1;
static double CUM1;
static double CKM1;
static double CMM1;
static double CVUM1;
static double CVHM1;
static double CHM1;
static double CVM1;
static double CALuM1;
static double CLuM1;
static double CVBrM1;
static double CBrtM1;
static double CBrbM1;
static double ABrM1;
static double CPM1;
static double CAM2;
static double CVSM2;
static double CSM2;
static double CVRM2;
static double CRM2;
static double CVFM2;
static double CVLM2;
static double CBrM1;
static double CFM2;
static double CLM2;
static double ABrM2;
static double CVKM2;
static double CKM2;
static double CUM2;
static double CVUM2;
static double CHM2;
static double CVHM2;
static double CALuM2;
static double CVMM2;
static double CMM2;
static double CVM2;
static double CLuM2;
static double CVBrM2;
static double CBrtM2;
static double CBrbM2;
static double CBrM2;
static double CPM2;

/* Initializers */

void initmod(void (* odeparms)(int *, double *)) {
  int N=66;
  odeparms(&N, parms);
}

void initforc(void (* odeforcs)(int *, double *)) {
  int N=1;
  odeforcs(&N, forc);
}

void getParms(double *inParms, double *out, int *nout) {
  int i;
  for (i = 0; i < *nout; i++) {
  	parms[i] = inParms[i];
  }

  BW = 70;

  /* Tissue fractions of BW (Brown 1997) */
  VRc = 0.09 - VLc - VLuc - VKc - VHc - VUc - VBrc;
  VSc = 0.746 - VFc - VMc;

  /* Tissue volumes (kg)*/
  VL = VLc * BW;
  VF = VFc * BW;
  VLu = VLuc * BW;
  VK = VKc * BW;
  VM = VMc * BW;
  VU = VUc * BW;
  VBr = VBrc * BW;
  VBrb = VBr * 0.05;
  VBrt = VBr * 0.95;
  VH = VHc * BW;
  VR = VRc * BW;
  VS = VSc * BW;
  VA = VAc * BW;
  VV = VVc * BW;

  /* Blood flow rate fractions (Brown 1997) */
  QSc = 0.24 - QFc - QMc;
  QRc = 0.76 - QLc - QKc - QHc - QUc - QBrc;

  /* Blood flow rates */
  QC = 15 * pow(BW, 0.74);
  QLu = QC;
  QL = QLc * QC;
  QF = QFc * QC;
  QK = QKc * QC;
  QM = QMc * QC;
  QU = QUc * QC;
  QBr = QBrc * QC;
  QH = QHc * QC;
  QR = QRc * QC;
  QS = QSc * QC;

  /*
   * ===============================================================================
   * Physicochemical parameters
   * ===============================================================================
   * P = Parent compound (CPF)
   * M1 = Metabolite 1 (CPF-oxon)
   * M2 = Metabolite 2 (TCPy)
   */

  /* Partition coefficient Parent (Predicted by WFSR tool) */
  PLP = 26.07 * BPP;
  PFP = 173.29 * BPP;
  PMP = 15.45 * BPP;
  PBrP = 50.56 * BPP;
  PLuP = 31.88 * BPP;
  PKP = 24.41 * BPP;
  PHP = 21.00 * BPP;
  PUP = PMP;
  PRP = PKP;
  PSP = PMP;

  /* Partition coefficient Metabolite 1(Predicted by WFSR tool) */
  PLM1 = 24.00 * BPM1;
  PFM1 = 125.68 * BPM1;
  PMM1 = 14.27 * BPM1;
  PUM1 = PMM1;
  PBrM1 = 46.50 * BPM1;
  PLuM1 = 29.32 * BPM1;
  PKM1 = 22.49 * BPM1;
  PHM1 = 19.34 * BPM1;
  PRM1 = PKM1;
  PSM1 = PMM1;

  /* Partition coefficient Metabolite 2 (Predicted by WFSR tool) */
  PLM2 = 0.15 * BPM2;
  PFM2 = 0.06 * BPM2;
  PMM2 = 0.13 * BPM2;
  PUM2 = PMM2;
  PBrM2 = 0.13 * BPM2;
  PLuM2 = 0.26 * BPM2;
  PKM2 = 0.19 * BPM2;
  PHM2 = 2.098 * BPM2;
  PRM2 = PKM2;
  PSM2 = PMM2;

  /* Brain Permeability surface area product */
  PSBrP = VBr * (3600 / 1000) * pow(10, (-2.06 + 0.448 * LogPP - 0.366 * MWP / 100));
  PSBrM1 = VBr * (3600 / 1000) * pow(10, (-2.06 + 0.448 * LogPM1 - 0.366 * MWM1 / 100));
  PSBrM2 = VBr * (3600 / 1000) * pow(10, (-2.06 + 0.448 * LogPM2 - 0.366 * MWM2 / 100));

  /* Pathway 1 (liver): CPF-->CPF-oxon, Vmax Pathway 1 */
  VMaxCYP1A2cP1 = VMaxCYP1A2mP1 * ISEFCYP1A2 * CYPabundanceCYP1A2;
  VMaxCYP2B6cP1 = VMaxCYP2B6mP1 * ISEFCYP2B6 * CYPabundanceCYP2B6;
  VMaxCYP2C19cP1 = VMaxCYP2C19mP1 * ISEFCYP2C19 * CYPabundanceCYP2C19;
  VMaxCYP3A4cP1 = VMaxCYP3A4mP1 * ISEFCYP3A4 * CYPabundanceCYP3A4;
  VMaxCYP1A2P1 = VMaxCYP1A2cP1 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP2B6P1 = VMaxCYP2B6cP1 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP2C19P1 = VMaxCYP2C19cP1 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP3A4P1 = VMaxCYP3A4cP1 / 1000000 * 60 * MPL * VL * 1000;

  /* Pathway 2 (liver): CPF-->TCPy, Vmax Pathway 2 */
  VMaxCYP1A2cP2 = VMaxCYP1A2mP2 * ISEFCYP1A2 * CYPabundanceCYP1A2;
  VMaxCYP2B6cP2 = VMaxCYP2B6mP2 * ISEFCYP2B6 * CYPabundanceCYP2B6;
  VMaxCYP2C19cP2 = VMaxCYP2C19mP2 * ISEFCYP2C19 * CYPabundanceCYP2C19;
  VMaxCYP3A4cP2 = VMaxCYP3A4mP2 * ISEFCYP3A4 * CYPabundanceCYP3A4;
  VMaxCYP1A2P2 = VMaxCYP1A2cP2 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP2B6P2 = VMaxCYP2B6cP2 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP2C19P2 = VMaxCYP2C19cP2 / 1000000 * 60 * MPL * VL * 1000;
  VMaxCYP3A4P2 = VMaxCYP3A4cP2 / 1000000 * 60 * MPL * VL * 1000;

  /* Pathway 3 (liver): CPF-oxon-->TCPy, Vmax Pathway 3 */
  VMax3 = VMax3c / 1000 * 60 * MPL * VL * 1000;

  /* Pathway 4 (blood): CPFO-->TCPy , Vmax Pathway 4 */
  VMax4 = VMax4c / 1000 * 60 * (VA + VV) * 0.55 * 1000;

  for (int i = 0; i < *nout; i++) {
    out[i] = parms[i];
  }
}

/* Events calculations */
void event(int *n, double *t, double *y) {
  y[Ast1] += forc[0] * Fa;
}

void derivs(int *neq, double *pdTime, double *y, double *ydot, double *yout, int *ip) {
  /*
   * ===============================================================================
   * Compartments of Parent compound
   * ===============================================================================
   */

  /* GI-tract compartment (repeated dose) (not added to blood flow) */

  ydot[Ast1] = - KaS * y[Ast1] - KsI * y[Ast1];	/* Amount of remaining in stomach (umol) */
  ydot[Ast2] = KsI * y[Ast1] - KaI * y[Ast2];   /* Amount remaining in intestine (umol) */

  /* Slowly perfused tissue compartment */
  ydot[ASP] = QS * (CAP - CVSP);		/* Amount in slowly perfused tissue (umol) */
  CSP = y[ASP] / VS;  /* Concentration in slowly perfused tissue (umol/L)*/
  CVSP = CSP / PSP;   /* Concentration leaving slowly perfused tissue with blood (umol/L)*/

  /* Richly perfused tissue compartment */
  ydot[ARP] = QR * (CAP - CVRP); /* Amount in richly perfused tissue (umol)*/
  CRP = y[ARP] / VR;  /* Concentration in richly perfused tissue (umol/L)*/
  CVRP = CRP / PRP;   /* Concentration leaving richly perfused tissue with blood (umol/L)*/

  /* Fat compartment */
  ydot[AFP] = QF * (CAP - CVFP); /* Amount in fat (umol) */
  CFP = y[AFP] / VF;  /* Concentration in fat (umol/L) */
  CVFP = CFP / PFP;   /* Concentration leaving fat with blood (umol/L)*/

  /* Liver compartment */
  ydot[ALP] =  QL * (CAP - CVLP) - ydot[ACPFO] - ydot[ATCPyA] + KaS * y[Ast1] + KaI * y[Ast2]; /* Amount in liver (umol)	*/
  CLP = y[ALP] / VL;  /* Concentration in liver (umol/L)*/
  CVLP = CLP / PLP;   /* Concentration leaving liver in blood (umol/L)*/

  ydot[ACPFO] = VMaxCYP1A2P1 * CVLP/(KmCYP1A2P1 + CVLP) + VMaxCYP2B6P1 * CVLP/(KmCYP2B6P1 + CVLP) + VMaxCYP2C19P1 * CVLP/(KmCYP2C19P1 + CVLP) + VMaxCYP3A4P1 * CVLP/(KmCYP3A4P1 + CVLP); /*Amount of Parent metabolized to Metabolite 1 in liver*/
  ydot[ATCPyA] = VMaxCYP1A2P2 * CVLP/(KmCYP1A2P2 + CVLP) + VMaxCYP2B6P2 * CVLP/(KmCYP2B6P2 + CVLP) + VMaxCYP2C19P2 * CVLP/(KmCYP2C19P2 + CVLP) + VMaxCYP3A4P2 * CVLP/(KmCYP3A4P2 + CVLP); /*Amount Parent metabolized to Metabolite 2 in liver */

  /* Kidney compartment */
  ydot[ACLP] = KurineP * y[AKP];  /* Amount cleared renally (umol)*/

  ydot[AKP] =  QK * (CAP - CVKP) - ydot[ACLP]; /* Amount in kidney (umol) */
  CKP = y[AKP] / VK;              /* Concentration in kidney (umol/L)*/
  CVKP = CKP / PKP;               /* Concentration leaving the kidney with blood (umol/L)*/

  /* Muscle tissue compartment */
  ydot[AMP] = QM * (CAP - CVMP);  /* Amount in muscle (umol)*/
  CMP = y[AMP] / VM;              /* Concentration in muscle (umol/L)*/
  CVMP = CMP / PMP;               /* Concentration leaving muscle with blood (umol/L)*/

  /* Uterus tissue compartment */
  ydot[AUP] = QU * (CAP - CVUP);  /* Amount in uterus (umol)*/
  CUP = y[AUP] / VM;              /* Concentration in uterus (umol/L)*/
  CVUP = CUP / PUP;               /* Concentration leaving uterus with blood (umol/L)*/

  /* Heart compartment */
  ydot[AHP] = QH * (CAP - CVHP);  /* Amount in heart (umol)*/
  CHP = y[AHP] / VH;              /* Concentration in heart (umol/L)*/
  CVHP = CHP / PHP;               /* Concentration leaving heart with blood (umol/L)*/

  /* Lung compartment */
  ydot[ALuP] = QLu * (CVP - CALuP); /* Amount in lung (umol)*/
  CLuP = y[ALuP] / VLu;             /* Concentration in lung (umol/L)*/
  CALuP = CLuP / PLuP;              /* Concentration leaving lung with blood (umol/L)*/

  /* Brain tissue compartment */
  ydot[ABrbP] = QBr * (CAP - CVBrP) - PSBrP * CVBrP + (PSBrP * CBrtP)/PBrP; /* Amount in brain blood (umol)*/
  CBrbP = y[ABrbP] / VBrb;      /* Concentration in brain blood (umol/L)*/

  ydot[ABrtP] = PSBrP * CVBrP - (PSBrP * CBrtP)/PBrP;	/* Amount in brain tissue (umol)*/
  CBrtP = y[ABrtP] / VBrt;      /* Concentration in brain tissue (umol/L)*/
  ABrP = y[ABrbP] + y[ABrtP];   /* Total amount in brain (umol)*/
  CBrP = ABrP/VBr;              /* Total concentration in brain (umol/L)*/
  CVBrP = CBrP / PBrP;          /* Concentration leaving brain with blood (umol/L)*/

  /* Blood compartment */
  ydot[AAP] = QC * (CALuP- CAP);  /* Amount in arterial blood (umol)*/
  CAP = y[AAP] / VA;              /* Concentration in arterial blood (umol)*/

  ydot[AVP] = (QF * CVFP + QR * CVRP + QS * CVSP + QL * CVLP + QK * CVKP + QH *CVHP + QM * CVMP+ QU * CVUP + QBr * CVBrP - QC * CVP); /* Amount in venous blood (umol)*/
  CVP = y[AVP] / VV;              /* Concentration in venous blood (umol/L)*/
  CPP = CVP * BPP;                /* Concentration in plasma (umol/L)*/

  yout[O_CVP] = CVP;
  yout[O_CPP] = CPP;

  /*===============================================================================
   * Compartments of Metabolite 1
   *===============================================================================
   */

  /* Slowly perfused tissue compartment */
  ydot[ASM1] = QS * (CAM1 - CVSM1); /* Amount in slowly perfused tissue (umol)*/
  CSM1 = y[ASM1] / VS; /* Concentration in slowly perfused tissue (umol/L)*/
  CVSM1 = CSM1 / PSM1; /* Concentration leaving slowly perfused tissue with blood (umol/L)*/

  /* Richly perfused tissue compartment */
  ydot[ARM1] = QR * (CAM1 - CVRM1); /* Amount in richly perfused tissue (umol)*/
  CRM1 = y[ARM1] / VR; /* Concentration in richly perfused tissue (umol/L)*/
  CVRM1 = CRM1 / PRM1; /* Concentration leaving richly perfused tissue with blood (umol/L)*/

  /* Fat compartment */
  ydot[AFM1] = QF * (CAM1 - CVFM1); /* Amount in fat (umol)*/
  CFM1 = y[AFM1] / VF; /* Concentration in fat (umol/L)*/
  CVFM1 = CFM1 / PFM1; /* Concentration leaving fat with blood (umol/L)*/

  /* Liver compartment */
  ydot[ALM1] =  QL * (CAM1-CVLM1) + ydot[ACPFO] - ydot[ATCPyB]; /* Amount in liver (umol)*/
  CLM1 = y[ALM1] / VL; /* Concentration in liver (umol/L)*/
  CVLM1 = CLM1 / PLM1; /* Concentration leaving liver with blood (umol/L)*/

  ydot[ATCPyB] = VMax3 * CVLM1/(Km3 + CVLM1); /* Amount Metabolite 1 metabolized to Metabolite 3  in liver */

  /* Kidney compartment */
  ydot[ACLM1] = KurineM1 * y[AKM1]; /* Amount cleared renally (umol)		*/
  ydot[AKM1] =  QK * (CAM1 - CVKM1) - ydot[ACLM1]; /* Amount in kidney (umol)   */
  CKM1 = y[AKM1] / VK;  /* Concentration in kidney (umol/L)*/
  CVKM1 = CKM1 / PKM1;  /* Concentration leaving kidney with blood (umol/L)*/

  /* Muscle tissue compartment */
  ydot[AMM1] = QM * (CAM1- CVMM1); /* Amount in muscle (umol)*/
  CMM1 = y[AMM1] / VM;    /* Concentration in muscle (umol/L)*/
  CVMM1 = CMM1 / PMM1;    /* Concentration leaving muscle with blood (umol/L)*/

  /* Uterus tissue compartment */
  ydot[AUM1] = QU * (CAM1- CVUM1); /* Amount in uterus (umol)*/
  CUM1 = y[AUM1] / VU;    /* Concentration in uterus (umol/L)*/
  CVUM1 = CUM1 / PUM1;    /* Concentration leaving uterus with blood (umol/L)*/

  /* Heart compartment */
  ydot[AHM1] = QH * (CAM1- CVHM1) 	; /* Amount in heart (umol)*/
  CHM1 = y[AHM1] / VH;    /* Concentration in heart (umol/L)*/
  CVHM1 = CHM1 / PHM1;    /* Concentration leaving heart with blood (umol/L)*/

  /* Lung compartment */
  ydot[ALuM1] = QLu * (CVM1 - CALuM1); /* Amount in lung (umol) */
  CLuM1 = y[ALuM1] / VLu; /* Concentration in lung (umol/L)*/
  CALuM1 = CLuM1 / PLuM1; /* Concentration leaving lung with blood (umol/L)*/

  /* Brain tissue compartment */
  ydot[ABrbM1] = QBr * (CAM1 - CVBrM1) - PSBrM1 * CVBrM1 + (PSBrM1 * CBrtM1) / PBrM1; /* Amount in brain blood (umol)*/
  CBrbM1 = y[ABrbM1] / VBrb; /* Concentration in brain blood (umol/L)*/

  ydot[ABrtM1]=  PSBrM1 * CVBrM1 - (PSBrM1 * CBrtM1) / PBrM1				; /*Amount in brain tissue (umol)*/
  CBrtM1 = y[ABrtM1] / VBrt; /* Concentration in brain tissue (umol/L)*/

  ABrM1 = y[ABrbM1] + y[ABrtM1];  /* Total amount in brain (umol)*/
  CBrM1 = ABrM1/VBr;              /* Total concentration in brain (umol/L)*/
  CVBrM1 = CBrM1 / PBrM1;         /* Concentration leaving fat with blood (umol/L)*/

  /* Blood compartment */
  ydot[AAM1] = QC * (CALuM1- CAM1); /* Amount in arterial blood (umol)*/
  CAM1 = y[AAM1] / VA;              /* Concentration in arterial blood (umol/L)*/

  ydot[AVM1] = (QF * CVFM1 + QR * CVRM1 + QS * CVSM1 + QL * CVLM1 + QK * CVKM1 + QH *CVHM1 + QM * CVMM1+ QU * CVUM1 + QBr * CVBrM1 - QC * CVM1) - ydot[ATCPyC]; /*  Amount in venous blood (umol)       */
  CVM1 = y[AVM1] / VV;  /* Concentration in venous blood (umol/L)*/
  CPM1 = CVM1 * BPM1;   /* Concentration in plasma (umol/L)*/

  yout[O_CVM1] = CVM1;
  yout[O_CPM1] = CPM1;

  ydot[ATCPyC] = VMax4 * CVM1/(Km4 + CVM1); /* Amount Metabolite 1 metabolized to Metabolite 2 in blood*/

  /* ===============================================================================
   * Compartments of Metabolite 2
   * ===============================================================================
   */

  /* Slowly perfused tissue compartment */
  ydot[ASM2] = QS * (CAM2 - CVSM2); /* Amount in slowly perfused tissue (umol)*/
  CSM2 = y[ASM2] / VS; /* Concentration in slowly perfused tissue (umol/L)*/
  CVSM2 = CSM2 / PSM2; /* Concentration leaving slowly perfused tissue with blood (umol/L)*/

  /* Richly perfused tissue compartment */
  ydot[ARM2] = QR * (CAM2 - CVRM2); /* Amount in richly perfused tissue (umol)*/
  CRM2 = y[ARM2] / VR; /* Concentration in richly perfused tissue (umol/L)	*/
  CVRM2 = CRM2 / PRM2; /* Concentration leaving richly perfused tissue with blood (umol/L)*/

  /* Fat compartment */
  ydot[AFM2] = QF * (CAM2 - CVFM2); /* Amount in fat (umol)*/
  CFM2 = y[AFM2] / VF; /* Concentration in fat (umol/L)*/
  CVFM2 = CFM2 / PFM2; /* Concentration leaving fat with blood (umol/L)*/

  /* Liver compartment -*/
  ydot[ALM2] =  QL * (CAM2 - CVLM2) + ydot[ATCPyA] + ydot[ATCPyB]; /* Amount in liver (umol) */
  CLM2 = y[ALM2] / VL; /* Concentration in liver (umol/L)*/
  CVLM2 = CLM2 / PLM2; /* Concentration leaving liver with blood (umol/L)*/

  /* Kidney compartment */
  ydot[ACLM2] = KurineM2 * (y[AFM2] + y[ASM2] + y[ARM2] + y[ALM2] + y[AVM2] + y[AAM2] + y[ALuM2] + y[AKM2] + y[AHM2] + y[AMM2] + y[AUM2] + ABrM2); /* Amount cleared renally (umol)*/

  ydot[AKM2] =  QK * (CAM2 - CVKM2) - ydot[ACLM2] ; /* Amount in kidney (umol)*/
  CKM2 = y[AKM2] / VK; /* Concentration in kidney (umol/L)*/
  CVKM2 = CKM2 / PKM2; /* Concentration leaving kidney with blood (umol/L)*/
  yout[O_ACLM2] = y[ACLM2];

  /* Muscle tissue compartment */
  ydot[AMM2] = QM * (CAM2- CVMM2); /* Amount in muscle (umol)*/
  CMM2 = y[AMM2] / VM; /* Concentration in muscle (umol/L)*/
  CVMM2 = CMM2 / PMM2; /* Concentration leaving muscle with blood (umol/L)*/

  /* Uterus tissue compartment -*/
  ydot[AUM2] = QU * (CAM2- CVUM2); /* Amount in uterus (umol)*/
  CUM2 = y[AUM2]/ VU;	/*Concentration in uterus (umol/L)*/
  CVUM2 = CUM2 / PUM2;	/*Concentration leaving uterus with blood (umol/L)*/

  /* Heart compartment */
  ydot[AHM2] = QH * (CAM2- CVHM2); /* Amount in heart (umol)*/
  CHM2 = y[AHM2] / VH; /* Concentration in heart (umol/L)*/
  CVHM2 = CHM2 / PHM2; /* Concentration leaving heart with blood (umol/L)*/

  /* Lung compartment */
  ydot[ALuM2] = QLu * (CVM2 - CALuM2); /* Amount in lung (umol)*/
  CLuM2 = y[ALuM2] / VLu; /* Concentration in lung (umol/L)*/
  CALuM2 = CLuM2 / PLuM2; /* Concentration leaving lung with blood (umol/L)*/

  /* Brain tissue compartment */
  ydot[ABrbM2] = QBr * (CAM2 - CVBrM2) - PSBrM2 * CVBrM2 + (PSBrM2 * CBrtM2) / PBrM2; /*Amount in brain blood (umol)*/
  CBrbM2 = y[ABrbM2] / VBrb; /*Concentration in brain blood (umol/L)*/

  ydot[ABrtM2]=  PSBrM2 * CVBrM2 - (PSBrM2 * CBrtM2)/PBrM2				; /*  Amount in brain tissue (umol)*/
  CBrtM2 = y[ABrtM2] / VBrt; /* Concentration in brain tissue (umol/L)*/

  ABrM2 = y[ABrbM2] + y[ABrtM2];  /* Total amount in brain (umol)*/
  CBrM2 = ABrM2/VBr;              /* Total concentration in brain (umol/L)*/
  CVBrM2 = CBrM2 / PBrM2;         /* Concentration leaving brain with blood (umol/L)*/

  /* Blood compartment */
  ydot[AAM2] = QC * (CALuM2- CAM2); /* Amount in arterial blood (umol)*/
  CAM2 =  y[AAM2] / VA;             /* Concentration in arterial blood (umol/L)*/

  ydot[AVM2] = (QF * CVFM2 + QR * CVRM2 + QS * CVSM2 + QL * CVLM2 + QK * CVKM2 + QH * CVHM2 + QM * CVMM2 + QU * CVUM2 + QBr * CVBrM2 - QC * CVM2) + ydot[ATCPyC]; /* Amount in venous blood (umol)   */
  CVM2 = y[AVM2] / VV;    /* Concentration in venous blood (umol/L)*/
  CPM2 = CVM2 * BPM2;     /* Amount in plasma (umol)*/

  yout[O_CVM2] = CVM2;
  yout[O_CPM2] = CPM2;
} /* derivs */


/*

https://mcb.berkeley.edu/courses/mcb137/exercises/madonnamanual.pdf

The Rosenbrock (stiff) method uses a semi-implicit fourth-order Runge-Kutta algorithm to
compute flows. It estimates error by comparing this flow with another flow computed using a
third-order algorithm. Again, the difference between these estimated flows is the estimated
error.
The Auto-stepsize method adjusts the stepsize by multiplying it by the following factor:
0.99 ¥
tol
e max
5 ,
where tol is the TOLERANCE and e max is the maximum relative error.
This factor is limited to the range 0.1 - 10; thus, the stepsize cannot change by more than a
factor of ten from one step to the next. The fifth root is used because, to a first approximation,
the change in the error is proportional to the change in stepsize raised to the fifth power for a
fourth-order method. The 0.99 is a “safety factor”: it’s much better to use a slightly smaller
stepsize than theoretically possible; otherwise, the algorithm may end up overestimating the
stepsize slightly and consequently discarding attempted steps (wasting time) because stepsize
was just a little too large.
The Rosenbrock (stiff) method adjusts the stepsize in the same way except that a different
factor is used:
0.99 ¥
tol
e max
4
This factor is limited to the range 0.5 - 2; thus, the stepsize cannot change by more than a factor
of two from one step to the next. The fourth root is used because the change in error is roughly
proportional to the change in stepsize raised to the fourth power for a third-order method.
For stiff systems of equations, the Auto-stepsize method grossly overestimates errors and thus
uses much smaller stepsizes than is actually necessary. The Rosenbrock (stiff) method does a
much better job for these types of systems.
On the other hand, for smooth, non-stiff systems, the Auto-stepsize method, being of higher
order than the Rosenbrock (stiff) method, can take larger steps for a given TOLERANCE. Also,
the Rosenbrock (stiff) method does a lot more math (matrix inversion, etc.) to estimate flows.
For these systems, the Auto-stepsize algorithm is a better choice.
The Auto-stepsize algorithm is derived from the routine rkqs() published in Numerical Recipes
in C. The Rosenbrock (stiff) method is derived from the routine stiff() in the same text.
*/

void printParameters() {
  Rprintf("  VRc %f\n", VRc);
  Rprintf("  VSc %f\n", VSc);
  Rprintf("  VL  %f\n", VL );
  Rprintf("  VF  %f\n", VF );
  Rprintf("  VLu %f\n", VLu);
  Rprintf("  VK  %f\n", VK );
  Rprintf("  VM  %f\n", VM );
  Rprintf("  VU  %f\n", VU );
  Rprintf("  VBr %f\n", VBr);
  Rprintf("  VBrb  %f\n", VBrb);
  Rprintf("  VBrt  %f\n", VBrt);
  Rprintf("  VH  %f\n", VH );
  Rprintf("  VR  %f\n", VR );
  Rprintf("  VS  %f\n", VS );
  Rprintf("  VA  %f\n", VA );
  Rprintf("  VV  %f\n", VV );
  Rprintf("  QSc %f\n", QSc);
  Rprintf("  QRc %f\n", QRc);
  Rprintf("  QC  %f\n", QC );
  Rprintf("  QLu %f\n", QLu);
  Rprintf("  QL  %f\n", QL );
  Rprintf("  QF  %f\n", QF );
  Rprintf("  QK  %f\n", QK );
  Rprintf("  QM  %f\n", QM );
  Rprintf("  QU  %f\n", QU );
  Rprintf("  QBr %f\n", QBr);
  Rprintf("  QH  %f\n", QH );
  Rprintf("  QR  %f\n", QR );
  Rprintf("  QS  %f\n", QS );
  Rprintf("  PLP %f\n", PLP);
  Rprintf("  PFP %f\n", PFP);
  Rprintf("  PMP %f\n", PMP);
  Rprintf("  PUP %f\n", PUP);
  Rprintf("  PBrP %f\n", PBrP);
  Rprintf("  PRP  %f\n", PRP );
  Rprintf("  PSP  %f\n", PSP );
  Rprintf("  PLuP %f\n", PLuP);
  Rprintf("  PKP  %f\n", PKP );
  Rprintf("  PHP  %f\n", PHP );
  Rprintf("  PLM1 %f\n", PLM1);
  Rprintf("  PFM1 %f\n", PFM1);
  Rprintf("  PMM1 %f\n", PMM1);
  Rprintf("  PUM1 %f\n", PUM1);
  Rprintf("  PBrM1  %f\n", PBrM1);
  Rprintf("  PRM1  %f\n", PRM1);
  Rprintf("  PSM1  %f\n", PSM1);
  Rprintf("  PLuM1  %f\n", PLuM1);
  Rprintf("  PKM1  %f\n", PKM1 );
  Rprintf("  PHM1  %f\n", PHM1 );
  Rprintf("  PLM2  %f\n", PLM2 );
  Rprintf("  PFM2  %f\n", PFM2 );
  Rprintf("  PMM2  %f\n", PMM2 );
  Rprintf("  PUM2  %f\n", PUM2 );
  Rprintf("  PBrM2 %f\n", PBrM2);
  Rprintf("  PRM2  %f\n", PRM2 );
  Rprintf("  PSM2  %f\n", PSM2 );
  Rprintf("  PLuM2 %f\n", PLuM2);
  Rprintf("  PKM2  %f\n", PKM2 );
  Rprintf("  PHM2  %f\n", PHM2 );
  Rprintf("  PSBrP %f\n", PSBrP);
  Rprintf("  PSBrM1  %f\n", PSBrM1);
  Rprintf("  PSBrM2  %f\n", PSBrM2);
  Rprintf("  VMaxCYP1A2cP1  %f\n", VMaxCYP1A2cP1  );
  Rprintf("  VMaxCYP2B6cP1  %f\n", VMaxCYP2B6cP1  );
  Rprintf("  VMaxCYP2C19cP1 %f\n", VMaxCYP2C19cP1 );
  Rprintf("  VMaxCYP3A4cP1  %f\n", VMaxCYP3A4cP1  );
  Rprintf("  VMaxCYP1A2P1   %f\n", VMaxCYP1A2P1   );
  Rprintf("  VMaxCYP2B6P1  %f\n", VMaxCYP2B6P1 );
  Rprintf("  VMaxCYP2C19P1  %f\n", VMaxCYP2C19P1  );
  Rprintf("  VMaxCYP3A4P1   %f\n", VMaxCYP3A4P1   );
  Rprintf("  VMaxCYP1A2cP2  %f\n", VMaxCYP1A2cP2  );
  Rprintf("  VMaxCYP2B6cP2  %f\n", VMaxCYP2B6cP2  );
  Rprintf("  VMaxCYP2C19cP2 %f\n", VMaxCYP2C19cP2 );
  Rprintf("  VMaxCYP3A4cP2  %f\n", VMaxCYP3A4cP2  );
  Rprintf("  VMaxCYP1A2P2  %f\n", VMaxCYP1A2P2 );
  Rprintf("  VMaxCYP2B6P2  %f\n", VMaxCYP2B6P2 );
  Rprintf("  VMaxCYP2C19P2  %f\n", VMaxCYP2C19P2  );
  Rprintf("  VMaxCYP3A4P2  %f\n", VMaxCYP3A4P2 );
  Rprintf("  VMax3  %f\n", VMax3);
  Rprintf("  VMax4  %f\n", VMax4);
}
