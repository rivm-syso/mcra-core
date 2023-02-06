/* model.c for MCRA PBPK models for bisphenols */

#include <R.h>

/* Model variables: States */
#define ID_Input_O 0x00000
#define ID_Input_D 0x00001
#define ID_Input_D2 0x00002
#define ID_AST 0x00003
#define ID_ASI 0x00004
#define ID_Afeces 0x00005
#define ID_AAO 0x00006
#define ID_AGImet 0x00007
#define ID_AGImets 0x00008
#define ID_Aoral 0x00009
#define ID_AGIBPAg 0x0000a
#define ID_AGIin 0x0000b
#define ID_AGIBPAs 0x0000c
#define ID_AGIins 0x0000d
#define ID_Aplasma 0x0000e
#define ID_AFat 0x0000f
#define ID_Agonad 0x00010
#define ID_Askin 0x00011
#define ID_ALiver 0x00012
#define ID_Amet_liver 0x00013
#define ID_Amet_livers 0x00014
#define ID_Abrain 0x00015
#define ID_AR 0x00016
#define ID_AS 0x00017
#define ID_Aurinebpa 0x00018
#define ID_ABPAg 0x00019
#define ID_ABPAg_prod_delay 0x0001a
#define ID_ABPAg_gut 0x0001b
#define ID_ABPAg_prod_delay_gut 0x0001c
#define ID_ABPAs 0x0001d
#define ID_ABPAs_prod_delay 0x0001e
#define ID_ABPAs_gut 0x0001f
#define ID_ABPAs_prod_delay_gut 0x00020
#define ID_ABPA_delay 0x00021
#define ID_ABPA_delayin 0x00022
#define ID_Afecesiv 0x00023
#define ID_ABPA_delayinbpag 0x00024
#define ID_Abpac 0x00025
#define ID_ABPA_delays 0x00026
#define ID_ABPA_delayins 0x00027
#define ID_Afecesivs 0x00028
#define ID_ABPA_delayinbpas 0x00029
#define ID_Abpasul 0x0002a
#define ID_Aurinebpag 0x0002b
#define ID_Areabsorption 0x0002c
#define ID_Aurineg 0x0002d
#define ID_Aurinebpas 0x0002e
#define ID_Areabsorptions 0x0002f
#define ID_Aurines 0x00030
#define ID_SSD 0x00031
#define ID_SSD2 0x00032

/* Model variables: Outputs */
#define ID_CPlasmaOut 0x00000
#define ID_CgonadOut 0x00001 /* Changed by Cecile */
#define ID_AurinebpaOut 0x00002 /* Changed by Cecile */
#define ID_AurinegOut 0x00003 /* Changed by Cecile */
#define ID_AurineTotalOut 0x00004 /* Changed by Cecile */

/*#define ID_Qtotal 0x00001
#define ID_Qbal 0x00002
*/

static double parms[263];  /* Changed by Cecile: parameters added specifically for age groups where needed */
/* Model variables: Parameters */
#define BW parms[0]
#define QCC parms[1]
#define QgonadC parms[2]
#define QliverC parms[3]
#define QfatC parms[4]
#define QbrainC parms[5]
#define QskinC parms[6]
#define QmuscleC parms[7]
#define VplasmaC parms[8]
#define VfatC parms[9]
#define VliverC parms[10]
#define VbrainC parms[11]
#define VskinC parms[12]
#define VgonadC parms[13]
#define VmuscleC parms[14]
#define VrichC parms[15]
#define VbodygC parms[16] 
#define VbodysC parms[17] 
#define MW parms[18]
#define pliver parms[19]
#define pfat parms[20]
#define pslow parms[21]
#define prich parms[22]
#define pgonad parms[23]
#define pbrain parms[24]
#define pskin parms[25]
#define geC parms[26]
#define k0C parms[27]
#define k1C parms[28]
#define k4C parms[29]
#define kGIingC parms[30]
#define kGIinsC parms[31]
#define kmgutg parms[32]
#define vmaxgutgC parms[33]
#define fgutg parms[34]
#define kmguts parms[35]
#define vmaxgutsC parms[36]
#define fguts parms[37]
#define met1g parms[38]
#define met1s parms[39]
#define enterocytes parms[40]
#define kmliver parms[41]
#define vmaxliverC parms[42]
#define fliverg parms[43]
#define kmlivers parms[44]
#define vmaxliversC parms[45]
#define flivers parms[46]
#define EHRtime parms[47]
#define EHRrateC parms[48]
#define k4C_IV parms[49]
#define kurinebpaC parms[50]
#define kurinebpagC parms[51]
#define kurinebpasC parms[52]
#define vreabsorptiongC parms[53]
#define vreabsorptionsC parms[54]
#define kreabsorptiong parms[55]
#define kreabsorptions parms[56]
#define kenterobpagC parms[57]
#define kenterobpasC parms[58]
#define D_o parms[59]
#define dose_O parms[60]
#define EoA_O parms[61]
#define uptake_O parms[62]
#define period_O parms[63]
#define koa parms[64]
#define t0_O parms[65]
#define t1_O parms[66]
#define D_d parms[67]
#define EoA_D parms[68]
#define dose_D parms[69]
#define aHL_D parms[70]
#define uptake_D parms[71]
#define period_D parms[72]
#define kda parms[73]
#define t0_D parms[74]
#define t1_D parms[75]
#define D_d2 parms[76]
#define EoA_D2 parms[77]
#define dose_D2 parms[78]
#define aHL_D2 parms[79]
#define uptake_D2 parms[80]
#define period_D2 parms[81]
#define kda2 parms[82]
#define t0_D2 parms[83]
#define t1_D2 parms[84]
#define QC parms[85]
#define Qfat parms[86]
#define Qliver parms[87]
#define Qgonad parms[88]
#define Qbrain parms[89]
#define Qskin parms[90]
#define Qslow parms[91]
#define Qrich parms[92]
#define Vliver parms[93]
#define Vfat parms[94]
#define Vgonad parms[95]
#define Vplasma parms[96]
#define Vbrain parms[97]
#define Vskin parms[98]
#define Vslow parms[99]
#define Vrich parms[100]
#define Vbodyg parms[101]
#define Vbodys parms[102]
#define BW075 parms[103]
#define BW025 parms[104]
#define vmaxliversCnew parms[105]
#define vmaxliverCnew parms[106]
#define vmaxgutgCnew parms[107]
#define vreabsorptiong parms[108]
#define vreabsorptions parms[109]
#define EHRrate parms[110]
#define k0 parms[111]
#define ge parms[112]
#define k1 parms[113]
#define k4 parms[114]
#define k4_IV parms[115]
#define vmaxliver parms[116]
#define kGIing parms[117]
#define met2g parms[118]
#define met2s parms[119]
#define kurinebpa parms[120]
#define kurinebpag parms[121]
#define kurinebpas parms[122]
#define vmaxlivers parms[123]
#define kGIins parms[124]
#define vmaxgutg parms[125]
#define vmaxguts parms[126]
#define kenterobpag parms[127]
#define kenterobpas parms[128]
#define t0_D1_day1  parms[129]
#define t0_D2_day1  parms[130]
#define t0_D1_day2  parms[131]
#define t0_D2_day2  parms[132]
#define t0_D1_day3  parms[133]
#define t0_D2_day3  parms[134]
#define t0_D1_day4  parms[135]
#define t0_D2_day4  parms[136]
#define t0_D21_day1 parms[137] 
#define t0_D22_day1 parms[138] 
#define t0_D21_day2 parms[139]
#define t0_D22_day2 parms[140]
#define t0_D21_day3 parms[141]
#define t0_D22_day3 parms[142]
#define t0_D21_day4 parms[143]
#define t0_D22_day4 parms[144]
#define t0_O1_day1  parms[145]
#define t0_O2_day1  parms[146]
#define t0_O3_day1  parms[147]
#define t0_O1_day2  parms[148]
#define t0_O2_day2  parms[149]
#define t0_O3_day2  parms[150]
#define t0_O1_day3  parms[151]
#define t0_O2_day3  parms[152]
#define t0_O3_day3  parms[153]
#define t0_O1_day4  parms[154] 
#define t0_O2_day4  parms[155]
#define t0_O3_day4  parms[156] 
#define ksiLiver parms[157] 
#define ksiGut parms[158]
#define age parms[159]
#define gender parms[160]
#define QCC_adult_f parms[161]
#define QgonadC_adult_f parms[162]
#define QliverC_adult_f parms[163]
#define QfatC_adult_f parms[164]
#define QbrainC_adult_f parms[165]
#define QskinC_adult_f parms[166]
#define QmuscleC_adult_f parms[167]
#define VplasmaC_adult_f parms[168]
#define VfatC_adult_f parms[169]
#define VliverC_adult_f parms[170]
#define VbrainC_adult_f parms[171]
#define VskinC_adult_f parms[172]
#define VgonadC_adult_f parms[173]
#define VmuscleC_adult_f parms[174]
#define VrichC_adult_f parms[175]
#define VbodygC_adult_f parms[176] 
#define VbodysC_adult_f parms[177] 
#define QCC_adult_m parms[178]
#define QgonadC_adult_m parms[179]
#define QliverC_adult_m parms[180]
#define QfatC_adult_m parms[181]
#define QbrainC_adult_m parms[182]
#define QskinC_adult_m parms[183]
#define QmuscleC_adult_m parms[184]
#define VplasmaC_adult_m parms[185]
#define VfatC_adult_m parms[186]
#define VliverC_adult_m parms[187]
#define VbrainC_adult_m parms[188]
#define VskinC_adult_m parms[189]
#define VgonadC_adult_m parms[190]
#define VmuscleC_adult_m parms[191]
#define VrichC_adult_m parms[192]
#define VbodygC_adult_m parms[193] 
#define VbodysC_adult_m parms[194]
#define QCC_adolescent_f parms[195]
#define QgonadC_adolescent_f parms[196]
#define QliverC_adolescent_f parms[197]
#define QfatC_adolescent_f parms[198]
#define QbrainC_adolescent_f parms[199]
#define QskinC_adolescent_f parms[200]
#define QmuscleC_adolescent_f parms[201]
#define VplasmaC_adolescent_f parms[202]
#define VfatC_adolescent_f parms[203]
#define VliverC_adolescent_f parms[204]
#define VbrainC_adolescent_f parms[205]
#define VskinC_adolescent_f parms[206]
#define VgonadC_adolescent_f parms[207]
#define VmuscleC_adolescent_f parms[208]
#define VrichC_adolescent_f parms[209]
#define VbodygC_adolescent_f parms[210] 
#define VbodysC_adolescent_f parms[211] 
#define QCC_adolescent_m parms[212]
#define QgonadC_adolescent_m parms[213]
#define QliverC_adolescent_m parms[214]
#define QfatC_adolescent_m parms[215]
#define QbrainC_adolescent_m parms[216]
#define QskinC_adolescent_m parms[217]
#define QmuscleC_adolescent_m parms[218]
#define VplasmaC_adolescent_m parms[219]
#define VfatC_adolescent_m parms[220]
#define VliverC_adolescent_m parms[221]
#define VbrainC_adolescent_m parms[222]
#define VskinC_adolescent_m parms[223]
#define VgonadC_adolescent_m parms[224]
#define VmuscleC_adolescent_m parms[225]
#define VrichC_adolescent_m parms[226]
#define VbodygC_adolescent_m parms[227] 
#define VbodysC_adolescent_m parms[228]
#define QCC_child_f parms[229]
#define QgonadC_child_f parms[230]
#define QliverC_child_f parms[231]
#define QfatC_child_f parms[232]
#define QbrainC_child_f parms[233]
#define QskinC_child_f parms[234]
#define QmuscleC_child_f parms[235]
#define VplasmaC_child_f parms[236]
#define VfatC_child_f parms[237]
#define VliverC_child_f parms[238]
#define VbrainC_child_f parms[239]
#define VskinC_child_f parms[240]
#define VgonadC_child_f parms[241]
#define VmuscleC_child_f parms[242]
#define VrichC_child_f parms[243]
#define VbodygC_child_f parms[244] 
#define VbodysC_child_f parms[245] 
#define QCC_child_m parms[246]
#define QgonadC_child_m parms[247]
#define QliverC_child_m parms[248]
#define QfatC_child_m parms[249]
#define QbrainC_child_m parms[250]
#define QskinC_child_m parms[251]
#define QmuscleC_child_m parms[252]
#define VplasmaC_child_m parms[253]
#define VfatC_child_m parms[254]
#define VliverC_child_m parms[255]
#define VbrainC_child_m parms[256]
#define VskinC_child_m parms[257]
#define VgonadC_child_m parms[258]
#define VmuscleC_child_m parms[259]
#define VrichC_child_m parms[260]
#define VbodygC_child_m parms[261] 
#define VbodysC_child_m parms[262]



/* Forcing (Input) functions */
static double forc[3];


/*----- Initializers */
void initmod (void (* odeparms)(int *, double *))
{
  int N=263; /* Changed by Cecile */
  odeparms(&N, parms);
}

void initforc (void (* odeforcs)(int *, double *))
{
  int N=4;
  odeforcs(&N, forc);
}


void getParms (double *inParms, double *out, int *nout) {
/* Selection of correct parametrization */

/*----- Model scaling */
/*
	printf("DEBUG = Initparms") ;
	*/
	
  int i;

  for (i = 0; i < *nout; i++) {
    parms[i] = inParms[i];
  }
	
    /*
	t1_O = t0_O + period_O;
   
    t1_D = t0_D + period_D;
   
    t1_D2 = t0_D2 + period_D2;
	*/
 if(age < 11 && gender==0){


	 QCC = QCC_child_f;
	 QgonadC = QgonadC_child_f;
	 QliverC = QliverC_child_f;
	 QfatC = QfatC_child_f;
	 QbrainC = QbrainC_child_f;
	 QskinC = QskinC_child_f;
	 QmuscleC = QmuscleC_child_f;
	 VplasmaC = VplasmaC_child_f;
	 VfatC = VfatC_child_f;
	 VliverC = VliverC_child_f;
	 VbrainC = VbrainC_child_f;
	 VskinC = VskinC_child_f;
	 VgonadC = VgonadC_child_f;
	 VmuscleC = VmuscleC_child_f;
	 VrichC = VrichC_child_f;
	 VbodygC = VbodygC_child_f;
	 VbodysC = VbodysC_child_f;
	 
	 } else if(age < 11 && gender==1){
	
	 QCC = QCC_child_m;
	 QgonadC = QgonadC_child_m;
	 QliverC = QliverC_child_m;
	 QfatC = QfatC_child_m;
	 QbrainC = QbrainC_child_m;
	 QskinC = QskinC_child_m;
	 QmuscleC = QmuscleC_child_m;
	 VplasmaC = VplasmaC_child_m;
	 VfatC = VfatC_child_m;
	 VliverC = VliverC_child_m;
	 VbrainC = VbrainC_child_m;
	 VskinC = VskinC_child_m;
	 VgonadC = VgonadC_child_m;
	 VmuscleC = VmuscleC_child_m;
	 VrichC = VrichC_child_m;
	 VbodygC = VbodygC_child_m;
	 VbodysC = VbodysC_child_m;	
	 } else if(age > 17 && gender==0){

	 QCC = QCC_adult_f;
	 QgonadC = QgonadC_adult_f;
	 QliverC = QliverC_adult_f;
	 QfatC = QfatC_adult_f;
	 QbrainC = QbrainC_adult_f;
	 QskinC = QskinC_adult_f;
	 QmuscleC = QmuscleC_adult_f;
	 VplasmaC = VplasmaC_adult_f;
	 VfatC = VfatC_adult_f;
	 VliverC = VliverC_adult_f;
	 VbrainC = VbrainC_adult_f;
	 VskinC = VskinC_adult_f;
	 VgonadC = VgonadC_adult_f;
	 VmuscleC = VmuscleC_adult_f;
	 VrichC = VrichC_adult_f;
	 VbodygC = VbodygC_adult_f;
	 VbodysC = VbodysC_adult_f; 
	 } else if(age > 17 && gender==1){
	 QCC = QCC_adult_m;
	 QgonadC = QgonadC_adult_m;
	 QliverC = QliverC_adult_m;
	 QfatC = QfatC_adult_m;
	 QbrainC = QbrainC_adult_m;
	 QskinC = QskinC_adult_m;
	 QmuscleC = QmuscleC_adult_m;
	 VplasmaC = VplasmaC_adult_m;
	 VfatC = VfatC_adult_m;
	 VliverC = VliverC_adult_m;
	 VbrainC = VbrainC_adult_m;
	 VskinC = VskinC_adult_m;
	 VgonadC = VgonadC_adult_m;
	 VmuscleC = VmuscleC_adult_m;
	 VrichC = VrichC_adult_m;
	 VbodygC = VbodygC_adult_m;
	 VbodysC = VbodysC_adult_m;
	 } else if(gender==0){
	 QCC = QCC_adolescent_f;
	 QgonadC = QgonadC_adolescent_f;
	 QliverC = QliverC_adolescent_f;
	 QfatC = QfatC_adolescent_f;
	 QbrainC = QbrainC_adolescent_f;
	 QskinC = QskinC_adolescent_f;
	 QmuscleC = QmuscleC_adolescent_f;
	 VplasmaC = VplasmaC_adolescent_f;
	 VfatC = VfatC_adolescent_f;
	 VliverC = VliverC_adolescent_f;
	 VbrainC = VbrainC_adolescent_f;
	 VskinC = VskinC_adolescent_f;
	 VgonadC = VgonadC_adolescent_f;
	 VmuscleC = VmuscleC_adolescent_f;
	 VrichC = VrichC_adolescent_f;
	 VbodygC = VbodygC_adolescent_f;
	 VbodysC = VbodysC_adolescent_f; 
	 }else{
	 QCC = QCC_adolescent_m;
	 QgonadC = QgonadC_adolescent_m;
	 QliverC = QliverC_adolescent_m;
	 QfatC = QfatC_adolescent_m;
	 QbrainC = QbrainC_adolescent_m;
	 QskinC = QskinC_adolescent_m;
	 QmuscleC = QmuscleC_adolescent_m;
	 VplasmaC = VplasmaC_adolescent_m;
	 VfatC = VfatC_adolescent_m;
	 VliverC = VliverC_adolescent_m;
	 VbrainC = VbrainC_adolescent_m;
	 VskinC = VskinC_adolescent_m;
	 VgonadC = VgonadC_adolescent_m;
	 VmuscleC = VmuscleC_adolescent_m;
	 VrichC = VrichC_adolescent_m;
	 VbodygC = VbodygC_adolescent_m;
	 VbodysC = VbodysC_adolescent_m; 
	 }
	 /*
 	 printf("DEBUG = age %f", age) ;	 
	 printf("DEBUG = gender %f", gender) ;
     printf("DEBUG = QCC %f", QCC) ;
	 printf("DEBUG = QgonadC %f", QgonadC) ;
	 */

  QC = QCC * 60 ;
  Qfat = QfatC * QC ;
  Qliver = QliverC * QC ;
  Qgonad = QgonadC * QC ;
  Qbrain = QbrainC * QC ;
  Qskin = QskinC * QC ;
  Qslow = QmuscleC * QC ;
  Qrich = QC - Qliver - Qbrain - Qfat - Qgonad - Qskin - Qslow ;

  Vliver = VliverC * BW ;
  Vfat = VfatC * BW ;
  Vgonad = VgonadC * BW ;
  Vbodyg = Vbodys = Vplasma = VplasmaC * BW ;
  Vbrain = VbrainC * BW ;
  Vskin = VskinC * BW ;
  Vslow = VmuscleC * BW ;
  Vrich = VrichC * BW ;


  BW075 =  exp(0.75*log(BW)) ;
  BW025 = exp(0.25*log(BW)) ;
  vmaxliversCnew = vmaxliversC * VliverC * 1000 ;
  vmaxliversCnew = vmaxliversCnew * BW / ( BW075 ) ;

  vmaxliverCnew = vmaxliverC * VliverC * 1000 ;
  vmaxliverCnew = vmaxliverCnew * BW / ( BW075 ) ;

  vmaxgutgCnew = vmaxgutgC * BW / ( BW075 ) ;

  vreabsorptiong = vreabsorptiongC * BW075 ;
  vreabsorptions = vreabsorptionsC * BW075 ;
  EHRrate = EHRrateC / ( BW025 ) ;
  k0 = k0C / BW025 ;
  ge = geC / BW025 ;
  k1 = k1C / BW025 ;
  k4 = k4C / BW025 ;
  k4_IV = k4C_IV / BW025 ;
  vmaxliver = vmaxliverCnew * fliverg * BW075 ;
  kGIing = kGIingC / BW025 ;
  met2g = 1.0 - met1g ;
  met2s = 1.0 - met1s ;
  kurinebpa = kurinebpaC * BW075 ;
  kurinebpag = kurinebpagC * BW075 ;
  kurinebpas = kurinebpasC * BW075 ;
  vmaxlivers = vmaxliversCnew * flivers * BW075 ;
  kGIins = kGIinsC / BW025 ;
  vmaxgutg = vmaxgutgCnew * fgutg * BW075 ;
  vmaxguts = vmaxgutsC * fguts * BW075 ;
  kenterobpag = kenterobpagC / BW025 ;
  kenterobpas = kenterobpasC / BW025 ;

  for (i = 0; i < *nout; i++) {
    out[i] = parms[i];
  }
  }
/*----- Dynamics section */

void derivs (int *neq, double *pdTime, double *y, double *ydot, double *yout, int *ip)
{
  /* local */ double kentero;
  /* local */ double dTPM;
  /* local */ double dPCP;
  /* local */ double dInput_D;
  /* local */ double dInput_D2;
  /* local */ double dSSD;
  /* local */ double dSSD2;
  /* local */ double dInput_O;
  /* local */ double Cgut;
  /* local */ double RST;
  /* local */ double RGImet;
  /* local */ double RGImets;
  /* local */ double Rfeces;
  /* local */ double RAO;
  /* local */ double RSI;
  /* local */ double Roral;
  /* local */ double RGIin;
  /* local */ double RGIBPAg;
  /* local */ double RGIins;
  /* local */ double RGIBPAs;
  /* local */ //double CFat;
  /* local */ double CVFat;
  /* local */ //double Cgonad;
  /* local */ double CVgonad;
  /* local */ //double Cskin;
  /* local */ double CVskin;
  /* local */ //double CLiver;
  /* local */ double CVLiver;
  /* local */ //double Cbrain;
  /* local */ double CVbrain;
  /* local */ //double CR;
  /* local */ double CVR;
  /* local */ double CVS;
  /* local */ //double CS;
  /* local */ double CV;
  /* local */ double CA;
  /* local */ double Rurinebpa;
  /* local */ double Rplasma;
  /* local */ double RAfat;
  /* local */ double RAgonad;
  /* local */ double RAskin;
  /* local */ double RAM;
  /* local */ double RAMs;
  /* local */ double Rbrain;
  /* local */ double RAR;
  /* local */ double RAS;
  /* local */ double RBPAg_prod;
  /* local */ double RBPAg_prod_delay;
  /* local */ double RBPAg_prod_gut;
  /* local */ double RBPAg_prod_delay_gut;
  /* local */ double RBPAs_prod;
  /* local */ double RBPAs_prod_delay;
  /* local */ double RBPAs_prod_gut;
  /* local */ double RBPAs_prod_delay_gut;
  /* local */ double RBPA_delayin;
  /* local */ double Rfecesiv;
  /* local */ double RBPA_delayinbpag;
  /* local */ double Cbpac;
  /* local */ double RBPA_delayins;
  /* local */ double Rfecesivs;
  /* local */ double RBPA_delayinbpas;
  /* local */ double Cbpas;
  /* local */ double Rreabsorption;
  /* local */ double Rurinebpag;
  /* local */ double Rurineg;
  /* local */ double Rreabsorptions;
  /* local */ double Rurinebpas;
  /* local */ double Rurines;
  /* local */ double Rbpas;
  /* local */ double Rbpac;
  /* local */ double RBPA_delay;
  /* local */ double RBPA_delays;
  /* local */ double RALiver;
  /* local */ //double conc;
  /* local */ double onoff_O1_day1;
  /* local */ double onoff_O2_day1;
  /* local */ double onoff_O3_day1;
  /* local */ double onoff_O1_day2;
  /* local */ double onoff_O2_day2;
  /* local */ double onoff_O3_day2;  
  /* local */ double onoff_O1_day3;
  /* local */ double onoff_O2_day3;
  /* local */ double onoff_O3_day3;   
  /* local */ double onoff_O1_day4;
  /* local */ double onoff_O2_day4;
  /* local */ double onoff_O3_day4;   
  /* local */ double onoff_D1_day1; 
  /* local */ double onoff_D2_day1; 
  /* local */ double onoff_D1_day2; 
  /* local */ double onoff_D2_day2; 
  /* local */ double onoff_D1_day3; 
  /* local */ double onoff_D2_day3;  
  /* local */ double onoff_D1_day4; 
  /* local */ double onoff_D2_day4;  
  /* local */ double onoff_D21_day1; 
  /* local */ double onoff_D22_day1; 
  /* local */ double onoff_D21_day2; 
  /* local */ double onoff_D22_day2; 
  /* local */ double onoff_D21_day3; 
  /* local */ double onoff_D22_day3;  
  /* local */ double onoff_D21_day4; 
  /* local */ double onoff_D22_day4; 



  if(pdTime[0] < EHRtime){kentero = 0 ;	} else{	kentero = EHRrate ;	}              
/* oral dosing */
          if(pdTime[0]<=(t0_O1_day1 + period_O) && pdTime[0]>=t0_O1_day1){ onoff_O1_day1 = 1 ; } else{ onoff_O1_day1 = 0 ; } 
          if(pdTime[0]<=(t0_O2_day1 + period_O) && pdTime[0]>=t0_O2_day1){ onoff_O2_day1 = 1 ; } else{ onoff_O2_day1 = 0 ; }
          if(pdTime[0]<=(t0_O3_day1 + period_O) && pdTime[0]>=t0_O3_day1){ onoff_O3_day1 = 1 ; } else{ onoff_O3_day1 = 0 ; }
          
          if(pdTime[0]<=(t0_O1_day2 + period_O) && pdTime[0]>=t0_O1_day2){ onoff_O1_day2 = 1 ; } else{ onoff_O1_day2 = 0 ; } 
          if(pdTime[0]<=(t0_O2_day2 + period_O) && pdTime[0]>=t0_O2_day2){ onoff_O2_day2 = 1 ; } else{ onoff_O2_day2 = 0 ; }
          if(pdTime[0]<=(t0_O3_day2 + period_O) && pdTime[0]>=t0_O3_day2){ onoff_O3_day2 = 1 ; } else{ onoff_O3_day2 = 0 ; }

          if(pdTime[0]<=(t0_O1_day3 + period_O) && pdTime[0]>=t0_O1_day3){ onoff_O1_day3 = 1 ; } else{ onoff_O1_day3 = 0 ; } 
          if(pdTime[0]<=(t0_O2_day3 + period_O) && pdTime[0]>=t0_O2_day3){ onoff_O2_day3 = 1 ; } else{ onoff_O2_day3 = 0 ; }
          if(pdTime[0]<=(t0_O3_day3 + period_O) && pdTime[0]>=t0_O3_day3){ onoff_O3_day3 = 1 ; } else{ onoff_O3_day3 = 0 ; }

          if(pdTime[0]<=(t0_O1_day4 + period_O) && pdTime[0]>=t0_O1_day4){ onoff_O1_day4 = 1 ; } else{ onoff_O1_day4 = 0 ; } 
          if(pdTime[0]<=(t0_O2_day4 + period_O) && pdTime[0]>=t0_O2_day4){ onoff_O2_day4 = 1 ; } else{ onoff_O2_day4 = 0 ; }
          if(pdTime[0]<=(t0_O3_day4 + period_O) && pdTime[0]>=t0_O3_day4){ onoff_O3_day4 = 1 ; } else{ onoff_O3_day4 = 0 ; }
/* dermal dosing from thermal paper */		  
          if(pdTime[0]<=(t0_D1_day1 + period_D) && pdTime[0]>=t0_D1_day1){ onoff_D1_day1 = 1 ; } else{ onoff_D1_day1 = 0 ; } 
          if(pdTime[0]<=(t0_D2_day1 + period_D) && pdTime[0]>=t0_D2_day1){ onoff_D2_day1 = 1 ; } else{ onoff_D2_day1 = 0 ; }
          
          if(pdTime[0]<=(t0_D1_day2 + period_D) && pdTime[0]>=t0_D1_day2){ onoff_D1_day2 = 1 ; } else{ onoff_D1_day2 = 0 ; } 
          if(pdTime[0]<=(t0_D2_day2 + period_D) && pdTime[0]>=t0_D2_day2){ onoff_D2_day2 = 1 ; } else{ onoff_D2_day2 = 0 ; }
          
          if(pdTime[0]<=(t0_D1_day3 + period_D) && pdTime[0]>=t0_D1_day3){ onoff_D1_day3 = 1 ; } else{ onoff_D1_day3 = 0 ; } 
          if(pdTime[0]<=(t0_D2_day3 + period_D) && pdTime[0]>=t0_D2_day3){ onoff_D2_day3 = 1 ; } else{ onoff_D2_day3 = 0 ; }
          
          if(pdTime[0]<=(t0_D1_day4 + period_D) && pdTime[0]>=t0_D1_day4){ onoff_D1_day4 = 1 ; } else{ onoff_D1_day4 = 0 ; } 
          if(pdTime[0]<=(t0_D2_day4 + period_D) && pdTime[0]>=t0_D2_day4){ onoff_D2_day4 = 1 ; } else{ onoff_D2_day4 = 0 ; }
/*  dermal dosing from PCPs */          
          if(pdTime[0]<=(t0_D21_day1 + period_D2)&& pdTime[0]>=t0_D21_day1){ onoff_D21_day1 = 1 ; } else{ onoff_D21_day1 = 0 ; }
          if(pdTime[0]<=(t0_D22_day1 + period_D2)&& pdTime[0]>=t0_D22_day1){ onoff_D22_day1 = 1 ; } else{ onoff_D22_day1 = 0 ; }
          
          if(pdTime[0]<=(t0_D21_day2 + period_D2)&& pdTime[0]>=t0_D21_day2){ onoff_D21_day2 = 1 ; } else{ onoff_D21_day2 = 0 ; }
          if(pdTime[0]<=(t0_D22_day2 + period_D2)&& pdTime[0]>=t0_D22_day2){ onoff_D22_day2 = 1 ; } else{ onoff_D22_day2 = 0 ; }
          
          if(pdTime[0]<=(t0_D21_day3 + period_D2)&& pdTime[0]>=t0_D21_day3){ onoff_D21_day3 = 1 ; } else{ onoff_D21_day3 = 0 ; } 
          if(pdTime[0]<=(t0_D22_day3 + period_D2)&& pdTime[0]>=t0_D22_day3){ onoff_D22_day3 = 1 ; } else{ onoff_D22_day3 = 0 ; }
          
          if(pdTime[0]<=(t0_D21_day4 + period_D2)&& pdTime[0]>=t0_D21_day4){ onoff_D21_day4 = 1 ; } else{ onoff_D21_day4 = 0 ; }
          if(pdTime[0]<=(t0_D22_day4 + period_D2)&& pdTime[0]>=t0_D22_day4){ onoff_D22_day4 = 1 ; } else{ onoff_D22_day4 = 0 ; }


  
 /* Dermal dosing Thermal paper */
   dTPM = kda * onoff_D1_day1 * EoA_D + kda * onoff_D2_day1  *  EoA_D  *  (t0_D2_day1 > 0) + 
										kda * onoff_D1_day2  *  EoA_D  *  (t0_D1_day2 > 0) + 
										kda * onoff_D2_day2  *  EoA_D  *  (t0_D2_day2 > 0) + 
										kda * onoff_D1_day3  *  EoA_D  *  (t0_D1_day3 > 0) + 
										kda * onoff_D2_day3  *  EoA_D  *  (t0_D2_day3 > 0) + 
										kda * onoff_D1_day4  *  EoA_D  *  (t0_D1_day4 > 0) + 
										kda * onoff_D2_day4  *  EoA_D  *  (t0_D2_day4 > 0) ;
          
/* Dermal dosing PCPs */
   dPCP = kda2 * onoff_D21_day1 * EoA_D2 + kda2 * onoff_D22_day1  *  EoA_D2  *  (t0_D22_day1 > 0) + 
										kda2 * onoff_D21_day2  *  EoA_D2  *  (t0_D21_day2 > 0) + 
										kda2 * onoff_D22_day2  *  EoA_D2  *  (t0_D22_day2 > 0) + 
										kda2 * onoff_D21_day3  *  EoA_D2  *  (t0_D21_day3 > 0) + 
										kda2 * onoff_D22_day3  *  EoA_D2  *  (t0_D22_day3 > 0) + 
										kda2 * onoff_D21_day4  *  EoA_D2  *  (t0_D21_day4 > 0) + 
										kda2 * onoff_D22_day4  *  EoA_D2  *  (t0_D22_day4 > 0) ;
 
  
  dInput_D = log ( 2 ) * ( 1 / aHL_D ) * y[ID_SSD] ;

  dInput_D2 = log ( 2 ) * ( 1 / aHL_D2 ) * y[ID_SSD2] ;

  dSSD = - dInput_D + dTPM ;

  dSSD2 = - dInput_D2 + dPCP ;
/* Dosing (oral) */    
  dInput_O = koa * onoff_O1_day1 +                      koa * onoff_O2_day1  *  (t0_O2_day1 > 0) + koa  *  onoff_O3_day1  *  (t0_O3_day1  > 0) + 
             koa * onoff_O1_day2  *  (t0_O1_day2 > 0) + koa * onoff_O2_day2  *  (t0_O2_day2 > 0) + koa  *  onoff_O3_day2  *  (t0_O3_day2  > 0) +
             koa * onoff_O1_day3  *  (t0_O1_day3 > 0) + koa * onoff_O2_day3  *  (t0_O2_day3 > 0) + koa  *  onoff_O3_day3  *  (t0_O3_day3  > 0) +
             koa * onoff_O1_day4  *  (t0_O1_day4 > 0) + koa * onoff_O2_day4  *  (t0_O2_day4 > 0) + koa  *  onoff_O3_day4  *  (t0_O3_day4  > 0) ;
			 
  
 
  /*
  printf("DEBUG = forcing %f", y[ID_Input_O]) ;
  printf("DEBUG = koa %f", koa) ;
  printf("DEBUG = t %f", pdTime[0]) ;
  printf("DEBUG = onoff %f", onoff_O) ;
  printf("DEBUG = dInput_O %f\n", dInput_O) ;
  */
  Cgut = y[ID_ASI] / enterocytes ;
   
  RST = dInput_O - k0 * y[ID_AST] - ge * y[ID_AST] ;
  
  if(ksiGut > 0){
	RGImet = vmaxgutg*Cgut/(kmgutg+Cgut+(Cgut*Cgut)/ksiGut) ;} 
  else{
	RGImet = vmaxgutg * Cgut / ( kmgutg + Cgut ) ;
	} /* Changed by Cecile */

  RGImets = vmaxguts * Cgut / ( kmguts + Cgut ) ;

  Rfeces = k4 * y[ID_ASI] ;
  RAO = k1 * y[ID_ASI] ;

  RSI = ge * y[ID_AST] - RGImet - RAO - RGImets ;

  Roral = k0 * y[ID_AST] + RAO ;

  RGIin = kGIing * y[ID_AGIBPAg] ;

  RGIBPAg = RGImet - RGIin ;

  RGIins = kGIins * y[ID_AGIBPAs] ;

  RGIBPAs = RGImets - RGIins ;

  //CFat = y[ID_AFat] / Vfat ;

  CVFat = y[ID_AFat] / ( Vfat * pfat ) ;

  //Cgonad = y[ID_Agonad] / Vgonad ;

  CVgonad = y[ID_Agonad] / ( Vgonad * pgonad ) ;

  //Cskin = y[ID_Askin] / Vskin ;

  CVskin = y[ID_Askin] / ( Vskin * pskin ) ;

  //CLiver = y[ID_ALiver] / Vliver ;

  CVLiver = y[ID_ALiver] / ( Vliver * pliver ) ;

  //Cbrain = y[ID_Abrain] / Vbrain ;

  CVbrain = y[ID_Abrain] / ( Vbrain * pbrain ) ;

  //CR = y[ID_AR] / Vrich ;

  CVR = y[ID_AR] / ( Vrich * prich ) ;

  CVS = y[ID_AS] / ( Vslow * pslow ) ;

  //CS = y[ID_AS] / Vslow ;

  CV = ( CVLiver * Qliver + CVskin * Qskin + CVFat * Qfat + CVR * Qrich + CVS * Qslow + CVgonad * Qgonad + CVbrain * Qbrain ) / QC ;

  CA = y[ID_Aplasma] / Vplasma ;
  	   
  Rurinebpa = kurinebpa * CV ;

  Rplasma = QC * ( CV - CA ) - Rurinebpa ;

  RAfat = Qfat * ( CA - CVFat ) ;

  RAgonad = Qgonad * ( CA - CVgonad ) ;

  RAskin = dInput_D + dInput_D2 + Qskin * ( CA - CVskin ) ;

  if(ksiLiver > 0){
	  RAM = vmaxliver*CVLiver/(kmliver+CVLiver+(CVLiver*CVLiver)/ksiLiver) ;} else{  RAM = vmaxliver * CVLiver / ( kmliver + CVLiver ) ; 
	} /* Changed by Cecile */

  RAMs = vmaxlivers * CVLiver / ( kmlivers + CVLiver ) ;

  Rbrain = Qbrain * ( CA - CVbrain ) ;

  RAR = Qrich * ( CA - CVR ) ;

  RAS = Qslow * ( CA - CVS ) ;

  RBPAg_prod = met1g * RAM ;

  RBPAg_prod_delay = met2g * RAM ;

  RBPAg_prod_gut = met1g * RGIin ;

  RBPAg_prod_delay_gut = met2g * RGIin ;

  RBPAs_prod = met1s * RAMs ;

  RBPAs_prod_delay = met2s * RAMs ;

  RBPAs_prod_gut = met1s * RGIins ;

  RBPAs_prod_delay_gut = met2s * RGIins ;

  RBPA_delayin = y[ID_ABPA_delay] * kentero ;

  Rfecesiv = y[ID_ABPA_delay] * k4_IV ;

  RBPA_delayinbpag = y[ID_ABPA_delay] * kenterobpag ;

  Cbpac = y[ID_Abpac] / ( Vbodyg +1E-34 ) ;

  RBPA_delayins = y[ID_ABPA_delays] * kentero ;

  Rfecesivs = y[ID_ABPA_delays] * k4_IV ;

  RBPA_delayinbpas = y[ID_ABPA_delays] * kenterobpas ;

  Cbpas = y[ID_Abpasul] / ( Vbodys +1E-34 ) ;

  Rreabsorption = vreabsorptiong * Cbpac / ( kreabsorptiong + Cbpac ) ;

  Rurinebpag = kurinebpag * Cbpac - Rreabsorption ;

  Rurineg = kurinebpag * Cbpac ;

  Rreabsorptions = vreabsorptions * Cbpas / ( kreabsorptions + Cbpas ) ;

  Rurinebpas = kurinebpas * Cbpas - Rreabsorptions ;

  Rurines = kurinebpas * Cbpas ;

  Rbpas = RBPAs_prod + RBPA_delayins + RBPAs_prod_gut - Rurinebpas ;

  Rbpac = RBPAg_prod + RBPAg_prod_gut + RBPA_delayin - Rurinebpag ;

  RBPA_delay = RBPAg_prod_delay + RBPAg_prod_delay_gut - RBPA_delayin - Rfecesiv - RBPA_delayinbpag ;

  RBPA_delays = RBPAs_prod_delay + RBPAs_prod_delay_gut - RBPA_delayins - Rfecesivs - RBPA_delayinbpas ;

  RALiver = Qliver * ( CA - CVLiver ) + Roral - RAM - RAMs + RBPA_delayinbpag + RBPA_delayinbpas ;
  
  ydot[ID_Input_O] = dInput_O ; 
 
  ydot[ID_Input_D] = dInput_D ;

  ydot[ID_Input_D2] = dInput_D2 ;
  
  ydot[ID_AST] = RST ;

  ydot[ID_ASI] = RSI ;

  ydot[ID_Afeces] = Rfeces ;

  ydot[ID_AAO] = RAO ;

  ydot[ID_AGImet] = RGImet ;

  ydot[ID_AGImets] = RGImets ;

  ydot[ID_Aoral] = Roral ;

  ydot[ID_AGIBPAg] = RGIBPAg ;

  ydot[ID_AGIin] = RGIin ;

  ydot[ID_AGIBPAs] = RGIBPAs ;

  ydot[ID_AGIins] = RGIins ;

  ydot[ID_Aplasma] = Rplasma ;

  ydot[ID_AFat] = RAfat ;

  ydot[ID_Agonad] = RAgonad ;

  ydot[ID_Askin] = RAskin ;

  ydot[ID_ALiver] = RALiver ;

  ydot[ID_Amet_liver] = RAM ;

  ydot[ID_Amet_livers] = RAMs ;

  ydot[ID_Abrain] = Rbrain ;

  ydot[ID_AR] = RAR ;

  ydot[ID_AS] = RAS ;

  ydot[ID_Aurinebpa] = Rurinebpa ;

  ydot[ID_ABPAg] = RBPAg_prod ;

  ydot[ID_ABPAg_prod_delay] = RBPAg_prod_delay ;

  ydot[ID_ABPAg_gut] = RBPAg_prod_gut ;

  ydot[ID_ABPAg_prod_delay_gut] = RBPAg_prod_delay_gut ;

  ydot[ID_ABPAs] = RBPAs_prod ;

  ydot[ID_ABPAs_prod_delay] = RBPAs_prod_delay ;

  ydot[ID_ABPAs_gut] = RBPAs_prod_gut ;

  ydot[ID_ABPAs_prod_delay_gut] = RBPAs_prod_delay_gut ;

  ydot[ID_ABPA_delay] = RBPA_delay ;

  ydot[ID_ABPA_delayin] = RBPA_delayin ;

  ydot[ID_Afecesiv] = Rfecesiv ;

  ydot[ID_ABPA_delayinbpag] = RBPA_delayinbpag ;

  ydot[ID_Abpac] = Rbpac ;

  ydot[ID_ABPA_delays] = RBPA_delays ;

  ydot[ID_ABPA_delayins] = RBPA_delayins ;

  ydot[ID_Afecesivs] = Rfecesivs ;

  ydot[ID_ABPA_delayinbpas] = RBPA_delayinbpas ;

  ydot[ID_Abpasul] = Rbpas ;

  ydot[ID_Aurinebpag] = Rurinebpag ;

  ydot[ID_Areabsorption] = Rreabsorption ;

  ydot[ID_Aurineg] = Rurineg ;

  ydot[ID_Aurinebpas] = Rurinebpas ;

  ydot[ID_Areabsorptions] = Rreabsorptions ;

  ydot[ID_Aurines] = Rurines ;

  ydot[ID_SSD] = dSSD ;

  ydot[ID_SSD2] = dSSD2 ;

  //conc = CV ;
  
  yout[ID_CPlasmaOut] = y[ID_Aplasma] / Vplasma ;
  
  yout[ID_CgonadOut] = y[ID_Agonad] / Vgonad ;  /* Changed by Cecile: concentration in gonads */
  
  yout[ID_AurinebpaOut] = y[ID_Aurinebpa] ; /* Changed by Cecile: cumulative excretion of BPA in urine */
  
  yout[ID_AurinegOut] = y[ID_Aurineg] ; /* Changed by Cecile: cumulative excretion of BPA-g in urine */
  
  yout[ID_AurineTotalOut] = y[ID_Aurinebpa] + y[ID_Aurineg] + y[ID_Aurines] ; /* Changed by Cecile: cumulative excretion of BPA and metabolites in urine */
  
  /*
  yout[ID_Qtotal] = Qliver + Qfat + Qrich + Qslow + Qgonad + Qbrain + Qskin ;
  yout[ID_Qbal] = yout[ID_Qtotal] - QC ;
  */

} /* derivs */


/*----- Jacobian calculations: */
void jac (int *neq, double *t, double *y, int *ml, int *mu, double *pd, int *nrowpd, double *yout, int *ip)
{

} /* jac */


/*----- Events calculations: */
void event (int *n, double *t, double *y)
{
	/*
	printf("Timesteps===== = t %f\n",t[0]) ;
    printf("forcing===== = t %f\n",forc[0]) ;
	*/
	koa = forc[0] + forc[1];
	kda =  forc[2] ;
	kda2 = forc[3] ;

} /* event */

/*----- Roots calculations: */
void root (int *neq, double *t, double *y, int *ng, double *gout, double *out, int *ip)
{

} /* root */

